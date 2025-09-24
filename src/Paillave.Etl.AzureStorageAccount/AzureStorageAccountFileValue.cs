using Paillave.Etl.Core;

namespace Paillave.Etl.AzureStorageAccount;

public class AzureStorageAccountFileValue(AzureBlobFileInfo fileInfo)
    : FileValueBase
{
    public override string Name => fileInfo.Name;

    public override Stream GetContent()
        => fileInfo.CreateReadStream();

    public override StreamWithResource OpenContent()
        => new(GetContent());

    protected override void DeleteFile()
        => fileInfo.DeleteAsync().Wait();
}
public class AzureStorageAccountFileValueMetadata
{
    public Uri? BaseUri { get; set; }
    public string? Folder { get; set; }
    public required string Name { get; set; }
    public string? DocumentContainer { get; set; }
}
