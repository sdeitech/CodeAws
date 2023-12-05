using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reveleer.SuperAdmin.AWS.ServiceContract
{
    public interface IS3Service
    {
        /// <summary>
        /// Checks S3 to see if the file exists based on file path
        /// File path contains the S3 bucket name and key
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        Task<bool> FileExists(string filePath);

        /// <summary>
        /// Checks S3 to see if the file exists based on Bucket Name and Key
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> FileExists(string bucketName, string key);

        /// <summary>
        /// Upload file Async request from S3.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bucketName"></param>
        /// <param name="fileKey"></param>
        /// <param name="tagSet"></param>
        /// <returns></returns>
        bool UploadFile(Stream stream, string bucketName, string fileKey, List<Tag> tagSet = null);

        /// <summary>
        /// Delete file from S3.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="fileKey"></param>
        Task<bool> DeleteFileAsync(string bucketName, string fileKey, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Download file from S3.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="storageFileKey"></param>
        /// <param name="downloadfilePath"></param>
        bool DownloadFile(string bucketName, string storageFileKey, string downloadfilePath);
    }
}
