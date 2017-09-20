using System.IO;
using System.Web.Hosting;

namespace FileManager
{
    /// <summary>
    /// A FileManager service that stores file to the server file system
    /// </summary>
    public class FileSystemService : IFileService
    {
        public string BaseUrlPath { get; set; }
        public string BaseDiskPath { get; set; }
        private string _folder;
        private string _baseUrlPath;
        private string _baseDiskPath;

        /// <summary>
        /// Constructor for FileSystemService
        /// </summary>
        /// <param name="baseUrlPath">The URL path by which to access the files saved</param>
        /// <param name="baseDiskPath">The path in which to save files on the server.  Default server mapping used when omitted.</param>
        public FileSystemService(string baseUrlPath, string baseDiskPath = "", string folder = "")
        {
            _baseUrlPath = baseUrlPath;
            _baseDiskPath = baseDiskPath;
            _folder = folder;
            SetFolder(folder);
        }

        public FileSystemService()
        {
        }

        public void SetFolder(string folder)
        {
            _folder = folder;
            BaseUrlPath = _baseUrlPath + "/" + folder;
            if (_baseDiskPath == "")
            {
                BaseDiskPath = HostingEnvironment.MapPath("~" + BaseUrlPath);
            }
            else
            {
                BaseDiskPath = _baseDiskPath + "/" + folder;
            }
        }

        /// <summary>
        /// Saves a Stream object representing a file to the server
        /// </summary>
        /// <param name="stream">The Stream object containing the file data to be saved</param>
        /// <param name="name">The filename under which to save the file</param>
        /// <returns>A URL by which the file can be accessed</returns>
        public string SaveFile(Stream stream, string name, bool timeStamped = false, string timeStamp = "")
        {
            if (stream.Length == 0)
            {
                throw new NoDataException("No data in stream");
            }

            string filePath = Path.Combine(BaseDiskPath, name);
            string fileUrl = BaseUrlPath + "/" + name;
            using (FileStream savingFile = File.Create(filePath))
            {
                stream.CopyTo(savingFile);
            }
            return fileUrl;
        }

        /// <summary>
        /// Deletes a file from the server
        /// </summary>
        /// <param name="filePath">The filename to be deleted</param>
        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        /// <summary>
        /// Deletes all files from the server matching a wildcard pattern
        /// </summary>
        /// <param name="filePath">The filepath of the files to be deleted including wildcards in the filename</param>
        public void DeleteFilesWithWildcard(string filePath)
        {
            Filepath filepath = new Filepath(filePath);

            // Can't delete files outside the scope of this FileSystemService
            if (!filepath.Path.Equals(BaseUrlPath))
            {
                throw new DirectoryNotFoundException("Invalid directory");
            }

            var files = Directory.GetFiles(BaseDiskPath, filepath.Filename);
            foreach (string file in files)
            {
                DeleteFile(file);
            }
        }
    }
}