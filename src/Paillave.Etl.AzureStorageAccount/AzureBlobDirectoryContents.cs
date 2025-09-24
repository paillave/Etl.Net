using System.Collections;
using Microsoft.Extensions.FileProviders;

namespace Paillave.Etl.AzureStorageAccount;

public class AzureBlobDirectoryContents : IDirectoryContents, IEnumerable<AzureBlobFileInfo>
{
    private readonly List<AzureBlobFileInfo> _blobs;
    public bool Exists { get; }
    internal AzureBlobDirectoryContents(List<AzureBlobFileInfo> blobs)
        => (_blobs, Exists) = (blobs, true);
    public IEnumerator<IFileInfo> GetEnumerator()
        => _blobs.ToList().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    IEnumerator<AzureBlobFileInfo> IEnumerable<AzureBlobFileInfo>.GetEnumerator()
        => _blobs.ToList().GetEnumerator();
}
