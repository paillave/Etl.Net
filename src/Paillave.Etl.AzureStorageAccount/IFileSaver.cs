using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Paillave.Etl.AzureStorageAccount;

public interface IFileSaver
{
    Task<IFileInfo> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string path,CancellationToken cancellationToken = default);
}
