using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Paillave.Etl.AzureStorageAccount;

internal static class BlobExtensions
{
    internal static BlobContainerClient GetBlobContainerClient(this AzureBlobOptions options)
        => options.GetBlobServiceClient().GetBlobContainerClient(options.DocumentContainer);
    private static BlobServiceClient GetBlobServiceClient(this AzureBlobOptions options)
    {
        if (options.ConnectionString != null)
            return new BlobServiceClient(options.ConnectionString);
        else if (options.BaseUri != null && (options.DefaultAzureCredential ?? false))
            return new BlobServiceClient(options.BaseUri, new DefaultAzureCredential(Environment.UserInteractive));
        else
            throw new ArgumentException("One of the following must be set: 'ConnectionString' or 'BaseUri'+'Token'!", nameof(options));
    }
    internal static AzureBlobFileInfo GetFileInfo(this BlobContainerClient container, string subpath)
        => new AzureBlobFileInfo(container.GetBlobClient(subpath));

    internal static AzureBlobDirectoryContents GetDirectoryContents(this BlobContainerClient container, string prefix, CancellationToken cancellationToken = default)
        => new AzureBlobDirectoryContents(container.ListDirectoryAsync(prefix, cancellationToken).ToBlockingEnumerable().ToList());
    internal static async IAsyncEnumerable<AzureBlobFileInfo> ListDirectoryAsync(this BlobContainerClient container, string prefix, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var resultSegment = container
            .GetBlobsByHierarchyAsync(BlobTraits.None, BlobStates.None, "/", prefix, cancellationToken)
            .AsPages(default, 100);
        await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
            foreach (BlobHierarchyItem blobHierarchyItem in blobPage.Values)
                yield return new AzureBlobFileInfo(container, blobHierarchyItem);
    }
    internal static async Task<AzureBlobFileInfo> SaveFileAsync(this BlobContainerClient container, string subpath, Stream stream, bool overwrite = false, IDictionary<string, string>? metadata = default, CancellationToken cancellationToken = default)
    {
        var blobClient = container.GetBlobClient(subpath);
        await blobClient.UploadAsync(stream, new BlobUploadOptions
        {
            Conditions = new BlobRequestConditions
            {
                IfNoneMatch = overwrite ? null : new ETag("*")
            },
            Metadata = metadata
        }, cancellationToken);
        return new AzureBlobFileInfo(blobClient);
    }
}
