using Paillave.Etl.Core;

namespace Paillave.Etl.AzureStorageAccountFileProvider;

public class AzureStorageAccountFileValue(AzureBlobFileInfo fileInfo, string connectorCode, string connectorName, string connectionName, AzureBlobOptions azureBlobOptions)
    : FileValueBase<AzureStorageAccountFileValueMetadata>(new AzureStorageAccountFileValueMetadata
    {
        BaseUri = azureBlobOptions.BaseUri,
        Name = fileInfo.Name,
        Folder = fileInfo.PhysicalPath,
        DocumentContainer = azureBlobOptions.DocumentContainer,
        ConnectionName = connectionName,
        ConnectorCode = connectorCode,
        ConnectorName = connectorName,
    })
{
    public override string Name => fileInfo.Name;

    public override Stream GetContent()
        => fileInfo.CreateReadStream();

    public override StreamWithResource OpenContent()
        => new(GetContent());

    protected override void DeleteFile()
        => fileInfo.DeleteAsync().Wait();
}
public class AzureStorageAccountFileValueMetadata : FileValueMetadataBase
{
    public Uri? BaseUri { get; set; }
    public string? Folder { get; set; }
    public required string Name { get; set; }
    public string? DocumentContainer { get; set; }
}
