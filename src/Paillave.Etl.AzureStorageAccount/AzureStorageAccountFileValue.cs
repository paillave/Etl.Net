using System;
using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.AzureStorageAccountFileProvider;

public class AzureStorageAccountFileValue : FileValueBase<AzureStorageAccountFileValueMetadata>
{
    private readonly AzureBlobOptions _azureBlobOptions;
    private readonly AzureBlobFileInfo _fileInfo;
    public AzureStorageAccountFileValue(AzureBlobFileInfo fileInfo, string connectorCode, string connectorName, string connectionName, AzureBlobOptions azureBlobOptions)
        : base(new AzureStorageAccountFileValueMetadata
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
        _fileInfo = fileInfo;
        _azureBlobOptions = azureBlobOptions;
    }

    public override string Name => _fileInfo.Name;

    public override Stream GetContent()
        => _fileInfo.CreateReadStream();

    public override StreamWithResource OpenContent()
        => new(GetContent());

    protected override void DeleteFile()
        => _fileInfo.DeleteAsync().Wait();
}
public class AzureStorageAccountFileValueMetadata : FileValueMetadataBase
{
    public Uri? BaseUri { get; set; }
    public string? Folder { get; set; }
    public required string Name { get; set; }
    public string? DocumentContainer { get; set; }
}
