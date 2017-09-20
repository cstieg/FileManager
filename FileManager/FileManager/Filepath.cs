using System;
using System.Linq;

namespace FileManager
{
    /// <summary>
    /// Represents a Filepath, allowing it to be broken down into the folder path and file name
    /// </summary>
    public class Filepath
    {
        private string _filePath;
        public Boolean IsBackslashes = false;

        /// <summary>
        /// Constructor for Filepath
        /// </summary>
        /// <param name="filePath">The filepath to be represented</param>
        public Filepath(string filePath)
        {
            _filePath = filePath;
        }

        public string FilePath
        {
            get
            {
                if (IsBackslashes)
                {
                    return _filePath.Replace('/', '\\');
                }
                return _filePath;
            }
            set
            {
                if (value.Contains('\\'))
                {
                    IsBackslashes = true;
                    value = value.Replace('\\', '/');
                }
                _filePath = value;
            }
        }

        public int Length
        {
            get
            {
                return FilePath.Length;
            }
        }

        public int PathLength
        {
            get
            {
                int lastSlash = FilePath.LastIndexOf('/');
                // If there is no slash, there is no path
                if (lastSlash == -1)
                {
                    return 0;
                }
                return FilePath.LastIndexOf('/');
            }
        }

        public int FilenameLength
        {
            get
            {
                int lastSlash = FilePath.LastIndexOf('/');
                // If there is no slash, the entire filepath is the filename
                if (lastSlash == -1)
                {
                    return Length;
                }
                return Length - FilePath.LastIndexOf('/') - 1;
            }
        }

        public string Filename
        {
            get
            {
                return FilePath.Substring(Length - FilenameLength);
            }
            set
            {
                FilePath = Path + "/" + value;
            }
        }

        public string Path
        {
            get
            {
                return FilePath.Substring(0, PathLength);
            }
            set
            {
                FilePath = value + "/" + Filename;
            }
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}