using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Paillave.Etl.Core;

namespace Paillave.Etl.S3;
public class S3Bucket : IDisposable
{
    private readonly IAmazonS3 _client;
    private readonly string _bucketName;

    public S3Bucket(AWSCredentials credentials, AmazonS3Config config, string bucketName)
    {
        _bucketName = bucketName;
        _client = new AmazonS3Client(credentials, config);
    }

    public async Task UploadAsync(string objectName, Stream stream, string? folder = null)
    {
        var response = await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = string.IsNullOrWhiteSpace(folder) ? objectName : StringEx.ConcatenatePath(folder, objectName),
            InputStream = stream
            // FilePath = filePath,
        });
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception();
        }
    }

    public Task<Stream> DownloadAsync(string objectName, string? folder = null)
        => DownloadFromKeyAsync(string.IsNullOrWhiteSpace(folder) ? objectName : StringEx.ConcatenatePath(folder, objectName));
    public async Task<Stream> DownloadFromKeyAsync(string objectKey)
    {
        GetObjectResponse response = await _client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
        });
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception();
        }
        return response.ResponseStream;
    }
    public Task<Stream> DownloadAsync(S3Object s3Object)
        => DownloadFromKeyAsync(s3Object.Key);

    public async Task<List<S3FileItem>> ListAsync(string? folder = null, bool? recursive = false)
    {
        List<S3FileItem> objects = new List<S3FileItem>();
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            MaxKeys = 100,
        };

        ListObjectsV2Response response;

        do
        {
            response = await _client.ListObjectsV2Async(request);

            var items = response.S3Objects
                .Select(i =>
                {
                    if (i.Key.EndsWith('/'))
                    {
                        return null;
                    }
                    var parent = Path.GetDirectoryName(i.Key);
                    string? fileName = Path.GetFileName(i.Key);
                    return new S3FileItem
                    {
                        FolderName = string.IsNullOrWhiteSpace(parent) ? null : parent,
                        Name = fileName,
                        S3Object = i
                    };
                })
                .Where(i => i != null)
                .Where(i =>
                {
                    if (string.IsNullOrWhiteSpace(folder))
                    {
                        if (recursive ?? false)
                        {
                            return true;
                        }
                        else
                        {
                            return i!.FolderName == null;
                        }
                    }
                    else
                    {
                        if (recursive ?? false)
                        {
                            return i!.FolderName?.Equals(folder, StringComparison.InvariantCultureIgnoreCase) ?? false;

                        }
                        else
                        {
                            return i!.FolderName?.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase) ?? false;
                        }
                    }
                })
                .ToList();
            objects.AddRange(items);

            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated);
        return objects;
    }

    public async Task DeleteAsync(string objectName)
    {
        var response = await _client.DeleteObjectAsync(_bucketName, objectName);
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK && response.HttpStatusCode != System.Net.HttpStatusCode.NoContent)
        {
            throw new Exception();
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
public class S3FileItem
{
    public string? FolderName { get; set; }
    public string Name { get; set; }
    public S3Object S3Object { get; set; }
}
