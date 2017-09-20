using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Deerfly_Patches.Modules.FileStorage.Amazon
{
    /// <summary>
    /// A file manager service to save files using Amazon S3 (Simple Storage Service)
    /// </summary>
    public class S3Service : IFileService
    {
        private string _domainName;
        private string _containerName;
        private string _accessKey = ConfigurationManager.AppSettings["AmazonS3AccessKey"];
        private string _secretKey = ConfigurationManager.AppSettings["AmazonS3SecretKey"];
        private string _regionName = ConfigurationManager.AppSettings["AmazonS3Region"];
        private AmazonS3Client s3Client;

        /// <summary>
        /// Constructor for S3Service which configures the service
        /// </summary>
        /// <param name="domainName">The domain name of the app, used to uniquely identify the bucket and avoid conflicts across global S3 namespace</param>
        /// <param name="containerName">The container (bucket) where this service stores files.  Set by folder param in wrapper.</param>
        public S3Service(string domainName, string containerName = "")
        {
            _domainName = domainName;
            if (containerName != "")
            {
                SetFolder(containerName);
            }

            AmazonS3Config config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(_regionName)
            };
            s3Client = new AmazonS3Client(_accessKey, _secretKey, config);
        }

        /// <summary>
        /// Sets the active bucket for this service, creates the bucket if it does not already exist.
        /// Will substitute dashes ("-") for unacceptable symbols such as /, \, and .
        /// </summary>
        /// <param name="folder">The name of the "folder" to become part of the bucket name.</param>
        public void SetFolder(string folder)
        {
            _containerName = _domainName + "/" + folder;
            _containerName = _containerName.Replace('/', '-').Replace('\\', '-').Replace('.', '-').Replace(' ', '-');

            CreateBucketIfNotExists(_containerName);
        }

        /// <summary>
        /// Gets the bucket object from the string bucket name
        /// </summary>
        /// <param name="bucketName">The name of the bucket to find among buckets owned by user</param>
        /// <returns>The bucket object, null if not found</returns>
        public S3Bucket GetBucket(string bucketName)
        {
            var bucketList = s3Client.ListBuckets().Buckets;
            return bucketList.Find(s => s.BucketName == bucketName);
        }

        /// <summary>
        /// Creates a bucket if the bucket does not exist
        /// </summary>
        /// <param name="bucketName">Name of the bucket to create.  Use only text and dashes, no slashes or periods.</param>
        /// <returns>The bucket object, whether preexisting or newly created</returns>
        public S3Bucket CreateBucketIfNotExists(string bucketName)
        {
            S3Bucket bucket = GetBucket(bucketName);
            if (bucket == null)
            {
                bucket = CreateBucket(bucketName);
            }
            return bucket;
        }

        /// <summary>
        /// Creates a bucket in the S3 service
        /// </summary>
        /// <param name="bucketName">Name of the bucket to create.  Use only text and dashes, no slashes or periods.</param>
        /// <returns>The bucket object</returns>
        public S3Bucket CreateBucket(string bucketName)
        {
            try
            {
                PutBucketRequest request = new PutBucketRequest()
                {
                    BucketName = bucketName,
                    UseClientRegion = true,
                    CannedACL = S3CannedACL.PublicRead
                };
                s3Client.PutBucket(request);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine(
                        "For service sign up go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine(
                        "Error occurred. Message:'{0}' when writing an object"
                        , amazonS3Exception.Message);
                }
            }
            return GetBucket(bucketName);
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
        /// Uploads a file to S3
        /// </summary>
        /// <param name="stream">A Stream object containing the file data to be uploaded</param>
        /// <param name="name">The filename where the file is to be uploaded</param>
        /// <returns>The URL where the uploaded file can be accessed</returns>
        public string UploadFile(Stream stream, string name)
        {
            try
            {
                using(TransferUtility transferUtility = new TransferUtility(s3Client))
                {
                    transferUtility.Upload(new TransferUtilityUploadRequest()
                    {
                        BucketName = _containerName,
                        Key = name,
                        InputStream = stream,
                        CannedACL = S3CannedACL.PublicRead
                    });
                }
                
                return "s3." + s3Client.Config.RegionEndpoint.SystemName + ".amazonaws.com/" + _containerName + "/" + name;
            }
            catch
            {
                throw new Exception("Failure to upload file");
            }
        }

        /// <summary>
        /// Gets a list of files in the current bucket
        /// </summary>
        /// <returns>A list of file objects contained by the current bucket</returns>
        public List<S3Object> GetFiles()
        {
            ListObjectsRequest request = new ListObjectsRequest()
            {
                BucketName = _containerName
            };
            ListObjectsResponse response = s3Client.ListObjects(request);
            return response.S3Objects;
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="filePath">The URL of the file to delete</param>
        public void DeleteFile(string filePath)
        {
            int fileNameStart = filePath.LastIndexOf("/");
            string fileName = filePath.Substring(fileNameStart + 1);
            DeleteObjectRequest request = new DeleteObjectRequest()
            {
                BucketName = _containerName,
                Key = fileName
            };
            s3Client.DeleteObject(request);
        }

        /// <summary>
        /// Deletes all files matching wildcard
        /// </summary>
        /// <param name="filePath">The URL of the file to delete containing wildcards</param>
        public void DeleteFilesWithWildcard(string filePath)
        {
            var fileList = GetFiles();
            fileList.ForEach(new Action<S3Object>(f =>
            {
                if (f.Key.Matches(filePath)) {
                    DeleteObjectRequest request = new DeleteObjectRequest()
                    {
                        BucketName = _containerName,
                        Key = f.Key
                    };
                    s3Client.DeleteObject(request);
                }
            }));
        }
    
    }
}
