using Azure.Storage.Blobs;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Paillave.Etl.AzureStorageAccountFileProvider;
public class AzureBlobFileProvider : IFileProvider, IFileSaver
{
    private readonly BlobContainerClient _blobContainerClient;

    public AzureBlobFileProvider(AzureBlobOptions azureBlobOptions)
        => _blobContainerClient = azureBlobOptions.GetBlobContainerClient();

    public IDirectoryContents GetDirectoryContents(string subpath)
        => _blobContainerClient.GetDirectoryContents(subpath);

    public IFileInfo GetFileInfo(string subpath)
        => _blobContainerClient.GetFileInfo(subpath);
    public async Task<IFileInfo> SaveFileAsync(string subpath, Stream stream, CancellationToken cancellationToken = default)
        => await _blobContainerClient.SaveFileAsync(subpath, stream, true, null, cancellationToken);
    public Task DeleteFileAsync(string subpath, CancellationToken cancellationToken = default)
        => _blobContainerClient.GetFileInfo(subpath).DeleteAsync(cancellationToken);

    private class AzureBlobFileProviderChangeToken : IChangeToken
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly string _filter;
        private readonly List<Action<object>> _callbacks = new();

        public AzureBlobFileProviderChangeToken(string filter, BlobContainerClient blobContainerClient)
            => (_filter, _blobContainerClient) = (filter, blobContainerClient);

        public bool ActiveChangeCallbacks => false;
        public bool HasChanged => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
        {
            _callbacks.Add(callback);
            return new DisposableAction(() => _callbacks.Remove(callback));
        }
    }
    public IChangeToken Watch(string filter) => new AzureBlobFileProviderChangeToken(filter, _blobContainerClient);


    private class DisposableAction : IDisposable
    {
        private readonly Action _action;
        public DisposableAction(Action action) => _action = action;
        public void Dispose() => _action();
    }
}
