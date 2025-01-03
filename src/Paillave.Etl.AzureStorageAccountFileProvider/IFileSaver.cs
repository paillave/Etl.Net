using Microsoft.Extensions.FileProviders;

namespace Paillave.Etl.AzureStorageAccountFileProvider;

public interface IFileSaver
{
    Task<IFileInfo> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string path,CancellationToken cancellationToken = default);
}
