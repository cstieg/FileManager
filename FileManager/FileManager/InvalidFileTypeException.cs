using System;

namespace FileManager
{
    /// <summary>
    /// The exception that is thrown when the file being saved is of an invalid type
    /// </summary>
    public class InvalidFileTypeException : Exception
    {
        public InvalidFileTypeException() : base() { }

        public InvalidFileTypeException(string message) : base(message) { }
    }
}