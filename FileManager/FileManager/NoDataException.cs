using System;

namespace FileManager
{
    /// <summary>
    /// The exception that is thrown when data that does not exist is attempted to be read
    /// </summary>
    public class NoDataException : Exception
    {
        public NoDataException() : base() { }

        public NoDataException(string message) : base(message) { }
    }
}