using System.IO;
using System.Web.Helpers;

namespace FileManager
{
    /// <summary>
    /// A utility to resize image streams
    /// </summary>
    public class ImageResizer
    {
        WebImage _webImage;

        /// <summary>
        /// Constructor for ImageResizer setting the image stream to manipulate.
        /// Creates a clone of the stream so not to consume the original
        /// </summary>
        /// <param name="imageStream">The image stream to manipulate</param>
        public ImageResizer(Stream imageStream )
        {
            _webImage = new WebImage(imageStream.CloneToMemoryStream());
        }

        /// <summary>
        /// Gets a stream containing the image resized to a given width
        /// </summary>
        /// <param name="width">The width in pixels which to resize the image</param>
        /// <returns>A stream with the resized image</returns>
        public Stream GetResizedImageStream(int width)
        {
            return GetResizedImage(width).GetImageStream();
        }

        /// <summary>
        /// Gets a WebImage object containing the image resized to a given width
        /// </summary>
        /// <param name="width">The width in pixels which to resize the image</param>
        /// <returns>The resized WebImage object</returns>
        public WebImage GetResizedImage(int width)
        {
            int originalWidth = _webImage.Width;
            int originalHeight = _webImage.Height;
            float aspectRatio = (float)(originalWidth) / originalHeight;
            int height = (int)(originalWidth / aspectRatio);
            WebImage resizedImage = _webImage.Resize(width, height, true);
            return resizedImage;
        }

        /// <summary>
        /// Saves the resized image to disk
        /// </summary>
        /// <param name="filePath">The filepath where to save the resized image</param>
        /// <param name="width">The width in pixels which to save the image</param>
        public void SaveResizedImage(string filePath, int width)
        {
            GetResizedImage(width);
            SaveAs(filePath);
        }

        /// <summary>
        /// Saves the image to disk
        /// </summary>
        /// <param name="filePath">The filepath where to save the image</param>
        public void SaveAs(string filePath)
        {
            _webImage.FileName = filePath;
            _webImage.Save();
        }

        /// <summary>
        /// Gets the width of the image in pixels
        /// </summary>
        /// <returns>The width of the image in pixels</returns>
        public int GetImageWidth()
        {
            return _webImage.Width;
        }

    }

    /// <summary>
    /// A helper class to add extensions to WebImage
    /// </summary>
    public static class WebImageHelper
    {
        /// <summary>
        /// An extension to convert the WebImage object to a Stream object
        /// </summary>
        /// <param name="webImage">The WebImage object to convert</param>
        /// <returns>A Stream object containing the image data</returns>
        public static Stream GetImageStream(this WebImage webImage)
        {
            return new MemoryStream(webImage.GetBytes());
        }
    }
}