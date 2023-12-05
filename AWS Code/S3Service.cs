using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Reveleer.Aws.SecretsManager;
using Reveleer.SuperAdmin.AWS.ServiceContract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reveleer.SuperAdmin.AWS.Service
{
    public class S3Service : IS3Service
    {
        private string _awsAccessKeyId = string.Empty;
        private string _awsSecretAccessKey = string.Empty;
        private string _awsRegion = string.Empty;
        private readonly IConfiguration _configuration;
        public static AwsSecretMetadata AwsSecretMetadata = null;

        public S3Service(IConfiguration configuration)
        {
            _configuration = configuration;
            loadConfigs();
        }
        /// <summary>
        /// Load AWS configuration information
        /// </summary>
        /// <returns></returns>
        private void loadConfigs()
        {
            _awsAccessKeyId = AwsSecretMetadata.AwsAccessKeyId;
            _awsSecretAccessKey = AwsSecretMetadata.AwsSecretAccessKey;
            _awsRegion = AwsSecretMetadata.AwsRegion;
        }
        /// <summary>
        /// Creates a instanc eof S3 client
        /// </summary>
        /// <returns></returns>
        private IAmazonS3 GetAmazonS3Client()
        {
            var options = new Amazon.Extensions.NETCore.Setup.AWSOptions();

            options.Profile = "default";
            options.Region = Amazon.RegionEndpoint.USEast1;
            options.Credentials = new Amazon.Runtime.BasicAWSCredentials(_awsAccessKeyId, _awsSecretAccessKey);

            var client = options.CreateServiceClient<IAmazonS3>();

            return client;
        }

        /// <summary>
        /// Checks S3 to see if the file exists based on file path
        /// File path contains the S3 bucket name and key
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<bool> FileExists(string filePath)
        {
            bool fileExists = false;

            BucketAndKey bucketAndKey = GetBucketAndKeyFromPath(filePath);

            fileExists = await FileExists(bucketAndKey.BucketName, bucketAndKey.Key);

            return fileExists;
        }

        /// <summary>
        /// Get bucket and key name from path
        /// </summary>
        /// <param name="filePath"></param>

        public static BucketAndKey GetBucketAndKeyFromPath(string filepath)
        {

            BucketAndKey bucketAndKey = new BucketAndKey();

            // List all objects
            string bucketName = string.Empty;
            string key = string.Empty;

            // In S3 the top level is a bucket, to search subfolders have to store the subfolder portion in prefix.
            char[] sep = new char[1] { '/' };

            string[] temp = filepath.Split(sep, 2);
            if (temp?.Length > 1)
            {
                bucketAndKey.BucketName = temp[0];
                bucketAndKey.Key = temp[1];
            }
            else
            {
                bucketAndKey.BucketName = filepath;
            };

            return bucketAndKey;

        }

        /// <summary>
        /// Checks S3 to see if the file exists based on Bucket Name and Key
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> FileExists(string bucketName, string key)
        {
            bool fileExists = false;

            try
            {
                var amazonS3Client = GetAmazonS3Client();

                var request = new GetObjectMetadataRequest()
                {
                    BucketName = bucketName,
                    Key = key
                };

                var response = await amazonS3Client
                                        .GetObjectMetadataAsync(request);

                if (response != null)
                {
                    fileExists = (response.HttpStatusCode == HttpStatusCode.OK);
                }
            }
            catch (Exception)
            {
                fileExists = false;
            }
            return fileExists;
        }

        /// <summary>
        /// Upload file Async request from S3.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bucketName"></param>
        /// <param name="fileKey"></param>
        /// <param name="tagSet"></param>
        /// <returns></returns>
        public bool UploadFile(Stream stream, string bucketName, string fileKey, List<Tag> tagSet = null)
        {

            bool retValue = false;

            var options = new Amazon.Extensions.NETCore.Setup.AWSOptions();

            options.Profile = "default";
            options.Region = Amazon.RegionEndpoint.USEast1;
            options.Credentials = new Amazon.Runtime.BasicAWSCredentials(_awsAccessKeyId, _awsSecretAccessKey);

            var client = options.CreateServiceClient<IAmazonS3>();
            Task<bool> task = UploadFileAsync(client, bucketName, stream, fileKey, tagSet);
            task.Wait();
            retValue = task.Result;

            return retValue;

        }

        /// <summary>
        /// Upload file Async request to S3.
        /// </summary>
        private static async Task<bool> UploadFileAsync(IAmazonS3 s3Client, string bucketName, Stream stream, string storageFileKey, List<Tag> tagSet, CancellationToken cancellationToken = default(CancellationToken))
        {

            try
            {
                var fileTransferUtility =
                    new TransferUtility(s3Client);

                TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = bucketName,
                    Key = storageFileKey,
                    InputStream = stream,
                    TagSet = tagSet,
                };

                await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);
                Console.WriteLine($"Upload  completed for {storageFileKey}");

            }

            catch (Exception ex)
            {
                return false;
            }

            return true;

        }

        /// <summary>
        /// Delete file from S3.
        /// </summary>
        public async Task<bool> DeleteFileAsync(string bucketName, string storageFileKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var options = new Amazon.Extensions.NETCore.Setup.AWSOptions();

                options.Profile = "default";
                options.Region = Amazon.RegionEndpoint.USEast1;
                options.Credentials = new Amazon.Runtime.BasicAWSCredentials(_awsAccessKeyId, _awsSecretAccessKey);

                var client = options.CreateServiceClient<IAmazonS3>();

                await client.DeleteAsync(bucketName, storageFileKey, null, cancellationToken);
                Console.WriteLine($"Upload  completed for {storageFileKey}");

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Download file from S3.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="storageFileKey"></param>
        /// <param name="downloadfilePath"></param>
        public bool DownloadFile(string bucketName, string storageFileKey, string downloadfilePath)
        {

            bool retValue = false;

            var options = new Amazon.Extensions.NETCore.Setup.AWSOptions();

            options.Profile = "default";
            options.Region = Amazon.RegionEndpoint.USEast1;
            options.Credentials = new Amazon.Runtime.BasicAWSCredentials(_awsAccessKeyId, _awsSecretAccessKey);

            var client = options.CreateServiceClient<IAmazonS3>();

            Task<bool> task = DownloadFileAsync(client, bucketName, storageFileKey, downloadfilePath);
            task.Wait();
            retValue = task.Result;

            return retValue;

        }

        /// <summary>
        /// Download file Async request from S3.
        /// </summary>
        /// <param name="s3Client"></param>
        /// <param name="bucketName"></param>
        /// <param name="storageFileKey"></param>
        /// <param name="downloadfilePath"></param>
        /// <returns></returns>
        private static async Task<bool> DownloadFileAsync(IAmazonS3 s3Client, string bucketName, string fileKey, string downloadfilePath)
        {
            try
            {
                var fileTransferUtility =
                    new TransferUtility(s3Client);

                await fileTransferUtility.DownloadAsync(downloadfilePath, bucketName, fileKey);
                Console.WriteLine($"Download  completed for {downloadfilePath}");

            }

            catch (Exception)
            {
                return false;
            }

            return true;

        }


    }
}
