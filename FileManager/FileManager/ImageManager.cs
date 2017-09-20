using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace FileManager
{
    /// <summary>
    /// A subclass of FileManager specifically handling images
    /// </summary>
    public class ImageManager : FileManager
    {
        protected string[] _validImageTypes = new string[]
        {
            "image/gif",
            "image/jpeg",
            "image/png"
        };
        protected List<int> _imageSizes = new List<int>()
        {
            1600, 800, 400, 200, 100
        };

        /// <summary>
        /// Constructor for ImageManager
        /// </summary>
        /// <param name="folder">The folder in which the images will be stored</param>
        /// <param name="storageService">The storage service used to store the images</param>
        /// <param name="validImageTypes">An optional string array of valid image types</param>
        /// <param name="imageSizes">An optional list of sizes to create when resizing images</param>
        public ImageManager(string folder, IFileService storageService = null, string[] validImageTypes = null, List<int> imageSizes = null) : base(folder, storageService)
        {
            if (validImageTypes != null)
            {
                _validImageTypes = validImageTypes;
            }
            if (imageSizes != null)
            {
                _imageSizes = imageSizes;
            }

            // sort descending
            _imageSizes.Sort(new Comparison<int>((i1, i2) => i2.CompareTo(i1)));
        }

        /// <summary>
        /// Saves an image derived from a POST request
        /// </summary>
        /// <param name="file">The image file derived from the POST request</param>
        /// <returns>The URL by which the image file is accessible</returns>
        public new string SaveFile(HttpPostedFileBase file, bool timeStamped = true, string timeStamp = "")
        {
            return SaveFile(file, null, timeStamped, timeStamp);
        }

        /// <summary>
        /// Saves an image derived from a POST request with a maximum width
        /// </summary>
        /// <param name="file">The image file derived from the POST request</param>
        /// <param name="maxWidth">The maximum width the image in pixels</param>
        /// <returns>The URL by which the image file is accessible, including a -w### extension indicating image width</returns>
        public string SaveFile(HttpPostedFileBase file, int? maxWidth, bool timeStamped = true, string timeStamp = "")
        {
            if (_validImageTypes.Count() > 0 && !_validImageTypes.Contains(file.ContentType))
            {
                throw new InvalidFileTypeException();
            }

            return SaveFile(file.InputStream, file.FileName, maxWidth, timeStamped, timeStamp);
        }

        /// <summary>
        /// Saves an image stream
        /// </summary>
        /// <param name="stream">The Stream object containing the image data</param>
        /// <param name="name">The name under which to save the images</param>
        /// <param name="maxWidth">The maximum width of the image in pixels</param>
        /// <returns>The URL by which the file is accessible</returns>
        public string SaveFile(Stream stream, string name, int? maxWidth, bool timeStamped = true, string timeStamp = "")
        {
            if (timeStamped)
            {
                // Timestamp the filename to prevent collisions
                name = GetTimeStampedFileName(name, timeStamp);
            }

            if (maxWidth != null)
            {
                ImageResizer imageResizer = new ImageResizer(stream);
                int imageWidth = imageResizer.GetImageWidth();
                if (imageWidth > maxWidth)
                {
                    Stream resizedStream = imageResizer.GetResizedImageStream((int)(maxWidth));
                    stream = resizedStream;
                    imageWidth = (int)(maxWidth);
                }
                name = GetResizedFileName(name, (int)(imageWidth));
            }

            // timeStamped = false, so not to double timestamp
            return base.SaveFile(stream, name, false);
        }

        /// <summary>
        /// Saves a file as images of multiple assorted sizes
        /// </summary>
        /// <param name="imageFile">The image file derived from a POST request</param>
        /// <param name="sizes">The list of image widths to be created</param>
        /// <returns>The URL by which the base file is accessible</returns>
        public string SaveImageMultipleSizes(HttpPostedFileBase imageFile, List<int> sizes = null, bool timeStamped = true, string timeStamp = "")
        {
            return SaveImageMultipleSizes(imageFile.InputStream, imageFile.FileName, sizes, timeStamped, timeStamp);
        }

        /// <summary>
        /// Saves a stream as images of multiple assorted sizes
        /// </summary>
        /// <param name="stream">The Stream containing the image data to be saved</param>
        /// <param name="name">The base image name</param>
        /// <param name="sizes">The list of image widths to be created</param>
        /// <returns>The URL by which the base file is accessible</returns>
        public string SaveImageMultipleSizes(Stream stream, string name, List<int> sizes = null, bool timeStamped = true, string timeStamp = "")
        {
            if (timeStamped)
            {
                // Timestamp the filename to prevent collisions
                name = GetTimeStampedFileName(name, timeStamp);
            }

            if (sizes == null)
            {
                sizes = _imageSizes;
            }

            List<string> srcSetItems = new List<string>();

            // For small images (smaller than largest desired width), remove target sizes that are larger than original image,
            // so as not to attempt expanding image.
            int imageWidth = new ImageResizer(stream).GetImageWidth();
            sizes = GetAdjustedSizeList(sizes, imageWidth);

            MemoryStream memoryStream = stream.CloneToMemoryStream();

            for (var i = 0; i < sizes.Count; i++)
            {
                // timeStamped = false, so not to double timestamp
                string url = SaveFile(memoryStream, name, sizes[i], false);
                srcSetItems.Add(url + " " + sizes[i] + "w");
            }

            string srcset = string.Join(", ", srcSetItems);
            return srcset;
        }

        /// <summary>
        /// Gets a file name suffixed with width information from the image
        /// </summary>
        /// <param name="filename">The base file name</param>
        /// <param name="size">The image width to be suffixed</param>
        /// <returns>The file name suffixed with width information from the image (filename + "-w###")</returns>
        public string GetResizedFileName(string filename, int size)
        {
            Filename imageFilename = new Filename(filename);
            return imageFilename.BaseName + "-w" + size.ToString() + imageFilename.Extension;
        }

        /// <summary>
        /// Gets a list of the sizes to be created for an image, removing sizes that are larger than the original
        /// </summary>
        /// <param name="sizes">The list of target image widths</param>
        /// <param name="originalWidth">The width of original image</param>
        /// <returns>An adjusted list of file widths to be created</returns>
        public List<int> GetAdjustedSizeList(List<int> sizes, int originalWidth)
        {
            List<int> adjustedSizes = new List<int>();
            Boolean isUndersizedImage = false;
            sizes.ForEach(s =>
            {
                if (s <= originalWidth)
                {
                    adjustedSizes.Add(s);
                }
                else
                { 
                    isUndersizedImage = true;
                }
            });
            // Add original (largest possible) size in case of small image
            if (isUndersizedImage)
            {
                adjustedSizes.Add(originalWidth);
            }
            return adjustedSizes;
        }

        /// <summary>
        /// Deletes multiple resized instances of an image file
        /// </summary>
        /// <param name="filePath">A sample of the filename to be deleted.  Ex: my_image-w800.jpg</param>
        public void DeleteImageWithMultipleSizes(string filePath)
        {
            Filepath filepath = new Filepath(filePath);
            Filename filename = new Filename(filepath.Filename);

            Regex findWidthTag = new Regex("-w[0-9]+$");
            filename.BaseName = findWidthTag.Replace(filename.BaseName, "-w*");
            filepath.Filename = filename.FileName;
            DeleteFilesWithWildcard(filepath.FilePath);
        }
    }
}