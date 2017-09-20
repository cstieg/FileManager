using ChristopherStieg.App_Start;
using System;
using System.IO;
using System.Web;

namespace FileManager
{
    /// <summary>
    /// A wrapper for the default file storage service
    /// </summary>
    public class FileManager : IFileManager
    {
        protected IFileService _storageService;
        protected string _folder;
        
        /// <summary>
        /// Constructor for FileManager which selects the file storage service to be used
        /// </summary>
        /// <param name="folder">The folder in which the files are to be saved</param>
        /// <param name="storageService">An IFileService object to serve as the storage service</param>
        public FileManager(string folder, IFileService storageService = null)
        {
            _folder = folder;

            // Get default storage service from RouteConfig if not specifically provided
            if (storageService == null)
            {
                _storageService = ContainerConfig.storageService;
            }
            else
            {
                _storageService = storageService;
            }
            _storageService.SetFolder(folder);
        }

        public void SetFolder(string folder)
        {
            _folder = folder;
        }

        /// <summary>
        /// Saves a posted file to the selected storage service
        /// </summary>
        /// <param name="file">The file to be saved, derived from a POST request</param>
        /// <returns>The URL by which the saved file is accessible</returns>
        public string SaveFile(HttpPostedFileBase file, bool timeStamped = true, string timeStamp = "")
        {
            if (file.InputStream.Length == 0)
            {
                throw new NoDataException("There is no data in this stream!");
            }

            return SaveFile(file.InputStream, file.FileName, timeStamped, timeStamp);
        }

        /// <summary>
        /// Saves a file stream to the selected storage service
        /// </summary>
        /// <param name="stream">The stream containing the file data to be saved</param>
        /// <param name="name">The filename by which to save the file</param>
        /// <returns></returns>
        public string SaveFile(Stream stream, string name, bool timeStamped = true, string timeStamp = "")
        {
            if (timeStamped)
            {
                // Timestamp the filename to prevent collisions
                name = GetTimeStampedFileName(name, timeStamp);
            }

            if (stream.Length == 0)
            {
                throw new NoDataException("There is no data in this stream!");
            }

            // Replace spaces with underscores for HTML access
            name = name.Replace(' ', '_');

            return _storageService.SaveFile(stream, name);
        }

        /// <summary>
        /// Deletes a file from the file storage service
        /// </summary>
        /// <param name="filePath">The name of the file to be deleted</param>
        public void DeleteFile(string filePath)
        {
            _storageService.DeleteFile(filePath);
        }

        /// <summary>
        /// Deletes all files which match the wildcard pattern from the file storage service
        /// </summary>
        /// <param name="filePath">The name of the files to be deleted including wildcards</param>
        public void DeleteFilesWithWildcard(string filePath)
        {
            _storageService.DeleteFilesWithWildcard(filePath);
        }

        public static string GetTimeStampedFileName(string name, string timeStamp = null)
        {
            if (timeStamp == null)
            {
                return GetTimeStamp() + "-" + name;
            }
            return timeStamp + "-" + name;
        }

        public static string GetTimeStamp()
        {
            return DateTime.Now.Year.ToString("D4") +
                DateTime.Now.Month.ToString("D2") +
                DateTime.Now.Day.ToString("D2") +
                DateTime.Now.Hour.ToString("D2") +
                DateTime.Now.Minute.ToString("D2") +
                DateTime.Now.Second.ToString("D2");
        }
    }
}