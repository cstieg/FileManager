using System;

namespace FileManager
{
    public class FileEmptyException : Exception
    {
        public FileEmptyException() : base() { }

        public FileEmptyException(string message = "File is empty") : base(message) { }
    }
}