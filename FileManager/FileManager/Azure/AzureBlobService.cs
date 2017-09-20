using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;

namespace Deerfly_Patches.Modules.FileStorage.Azure
{
    /// <summary>
    /// A file manager service to save files using Azure Blob
    /// </summary>
    public class AzureBlobService : IFileService
    {
        private string _connectionString;
        private string _containerName;
        private CloudBlobContainer _blobContainer;

        /// <summary>
        /// Constructor for AzureBlobService which configures the service
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to Azure Blob</param>
        /// <param name="containerName">The container where this service stores files.  Set by folder param in wrapper.</param>
        public AzureBlobService(string connectionString, string containerName = "")
        {
            _connectionString = connectionString;
            if (containerName != "")
            {
                SetFolder(containerName);
            }
        }

        public void SetFolder(string folder)
        {
            _containerName = folder;
            ConfigureBlobContainer();
            _blobContainer = GetContainer();
        }

        /// <summary>
        /// An alias for UploadFile to fulfill the IFileManager interface
        /// </summary>
        /// <param name="stream">A Stream object containing the file data to be saved</param>
        /// <param name="name">The filename where the file is to be saved</param>
        /// <returns>The URL where the saved file can be accessed</returns>
        public string SaveFile(Stream stream, string name, bool timeStamped = false, string timeStamp = "")
        {
            if (stream.Length != 0)
            {
                return UploadFile(stream, name);
            }
            throw new FileEmptyException();
        }

        /// <summary>
        /// Uploads a file to Azure Blob
        /// </summary>
        /// <param name="stream">A Stream object containing the file data to be uploaded</param>
        /// <param name="name">The filename where the file is to be uploaded</param>
        /// <returns>The URL where the uploaded file can be accessed</returns>
        public string UploadFile(Stream stream, string name)
        {
            try
            {   
                // get blob
                CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(name);

                // upload file
                blob.UploadFromStream(stream);
                SetPublicContainerPermissions(_blobContainer);
                return blob.Uri.AbsoluteUri;
            }
            catch
            {
                throw new AzureBlobException("Failure to upload file to blob");
            }
        }

        /// <summary>
        /// Deletes a blob
        /// </summary>
        /// <param name="filePath">The URL of the blob to delete</param>
        public void DeleteFile(string filePath)
        {
            // TODO
        }

        /// <summary>
        /// Deletes all blobs matching wildcard
        /// </summary>
        /// <param name="filePath">The URL of the blob to delete containing wildcards</param>
        public void DeleteFilesWithWildcard(string filePath)
        {
            // TODO
        }

        /// <summary>
        /// Makes a blob container publicly accessible
        /// </summary>
        /// <param name="blobContainer">The blob container to be made publicly accessible</param>
        public void SetPublicContainerPermissions(CloudBlobContainer blobContainer)
        {
            BlobContainerPermissions permissions = blobContainer.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            blobContainer.SetPermissions(permissions);
        }

        /// <summary>
        /// Configures the blob container
        /// </summary>
        private void ConfigureBlobContainer()
        {
            try
            {
                // create blob container if does not exist and configure to be public
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(_connectionString));
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(_containerName);
                if (blobContainer.CreateIfNotExists())
                {
                    SetPublicContainerPermissions(blobContainer);
                    //log.Information("Successfully created public blob storage container");
                }
            }
            catch
            {
                throw new AzureBlobException("Failure to create or configure blob storage service");
            }

        }

        /// <summary>
        /// Gets the blob container for the service
        /// </summary>
        /// <returns>A blob container with the name specified in the service</returns>
        private CloudBlobContainer GetContainer()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(_connectionString));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(_containerName);

            return blobContainer;
        }

    }
}