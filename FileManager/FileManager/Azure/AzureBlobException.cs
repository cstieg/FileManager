using System;

namespace Deerfly_Patches.Modules.FileStorage.Azure
{
    /// <summary>
    /// The exception that is thrown when Azure Blob encounters a file IO error
    /// </summary>
    public class AzureBlobException : Exception
    {
        public AzureBlobException() : base() { }

        public AzureBlobException(string message) : base(message) { }
    }
}