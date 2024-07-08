using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

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

    public async Task UploadAsync(string objectName, Stream stream)
    {
        var response = await _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = objectName,
            InputStream = stream
            // FilePath = filePath,
        });
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception();
        }
    }

    public async Task<Stream> DownloadAsync(string objectName)
    {
        GetObjectResponse response = await _client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = objectName,
        });
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception();
        }
        return response.ResponseStream;
    }

    public async Task<List<S3Object>> ListAsync()
    {
        List<S3Object> objects = new List<S3Object>();
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            MaxKeys = 100,
        };

        ListObjectsV2Response response;

        do
        {
            response = await _client.ListObjectsV2Async(request);
            objects.AddRange(response.S3Objects);

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
