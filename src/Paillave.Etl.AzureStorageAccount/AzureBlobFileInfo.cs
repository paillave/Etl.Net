using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.FileProviders;

namespace Paillave.Etl.AzureStorageAccount;

public class AzureBlobFileInfo : IFileInfo
{
    private readonly BlobClient? _blobClient = null;
    private readonly BlobContainerClient? _blobContainerClient = null;
    internal AzureBlobFileInfo(BlobClient blobClient)
    {
        _blobClient = blobClient;

        var properties = blobClient.GetProperties()?.Value ?? throw new InvalidOperationException("Cannot get blob properties.");
        Name = blobClient.Name.Split('/').Last();
        Length = properties.ContentLength;
        LastModified = properties.LastModified;
        PhysicalPath = blobClient.Name;
    }
    internal AzureBlobFileInfo(BlobContainerClient blobContainerClient, BlobHierarchyItem blobHierarchyItem)
    {
        _blobContainerClient = blobContainerClient;
        if (blobHierarchyItem.IsPrefix)
        {
            IsDirectory = true;
            Name = blobHierarchyItem.Prefix.TrimEnd('/').Split('/').Last();
            PhysicalPath = blobHierarchyItem.Prefix;
        }
        else
        {
            _blobClient = blobContainerClient.GetBlobClient(blobHierarchyItem.Blob.Name);
            Name = blobHierarchyItem.Blob.Name.Split('/').Last();
            Length = blobHierarchyItem.Blob.Properties.ContentLength ?? 0;
            LastModified = blobHierarchyItem.Blob.Properties.LastModified ?? DateTimeOffset.MinValue;
            PhysicalPath = blobHierarchyItem.Blob.Name;
        }
    }
    public async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        if (_blobClient == null)
            throw new InvalidOperationException("Cannot delete a directory.");
        await _blobClient.DeleteAsync(cancellationToken: cancellationToken);
    }
    public async Task SaveStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (_blobClient == null)
            throw new InvalidOperationException("Cannot save a stream for a directory.");
        await _blobClient.UploadAsync(stream, cancellationToken);
        var properties = _blobClient.GetProperties()?.Value ?? throw new InvalidOperationException("Cannot get blob properties.");
        Length = properties.ContentLength;
        LastModified = properties.LastModified;
    }
    public Task<AzureBlobFileInfo> SaveStream(string name, Stream stream, CancellationToken cancellationToken = default)
    {
        if (_blobContainerClient == null)
            throw new InvalidOperationException("Cannot save a stream for a file.");
        return _blobContainerClient.SaveFileAsync($"{PhysicalPath}{name}", stream, true, null, cancellationToken);
    }
    public Stream CreateReadStream() => _blobClient != null
        ? _blobClient.OpenRead()
        : throw new InvalidOperationException("Cannot open a stream for a directory!");

    public bool Exists => true;
    public long Length { get; private set; }
    public string? PhysicalPath { get; }
    public string Name { get; }
    public DateTimeOffset LastModified { get; private set; }
    public bool IsDirectory { get; }
}
