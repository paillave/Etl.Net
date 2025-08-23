using System.Text.Json;
using Microsoft.Extensions.FileSystemGlobbing;
using Paillave.Etl.Core;

namespace Paillave.Etl.AzureStorageAccountFileProvider;

public class AzureStorageAccountAdapterProviderParameters
{
    public string? SubFolder { get; set; }
    public string? FileNamePattern { get; set; }
}

public class AzureStorageAccountFileValueProvider(string code, string name, string connectionName,
                                 AzureBlobOptions connectionParameters,
                                 AzureStorageAccountAdapterProviderParameters inputParameters)
    : FileValueProviderBase<AzureBlobOptions, AzureStorageAccountAdapterProviderParameters>(code, name, connectionName, connectionParameters, inputParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    public override IFileValue Provide(string name, string fileSpecific)
    {
        return new AzureStorageAccountFileValue(new AzureBlobFileInfo(connectionParameters.GetBlobContainerClient().GetBlobClient(fileSpecific)), this.Code, this.Name, this.ConnectionName, connectionParameters);
    }
    protected override void Provide(object input, Action<IFileValue, FileReference> pushFileValue, AzureBlobOptions azureBlobOptions,
                                    AzureStorageAccountAdapterProviderParameters providerParameters, CancellationToken cancellationToken)
    {
        var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        var matcher = new Matcher().AddInclude(searchPattern);
        var blobContainerClient = azureBlobOptions.GetBlobContainerClient();
        foreach (var blobHierarchyItem in blobContainerClient.GetDirectoryContents(providerParameters.SubFolder ?? string.Empty, cancellationToken).Cast<AzureBlobFileInfo>())
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (!blobHierarchyItem.IsDirectory)
            {
                if (matcher.Match(blobHierarchyItem.Name).HasMatches)
                {
                    var fileValue = new AzureStorageAccountFileValue(blobHierarchyItem, this.Code, this.Name, this.ConnectionName, azureBlobOptions);
                    var fileReference = new FileReference(fileValue.Name, this.Code, blobHierarchyItem.Name);
                    pushFileValue(fileValue, fileReference);
                }
            }
        }
    }

    protected override void Test(AzureBlobOptions connectionParameters, AzureStorageAccountAdapterProviderParameters inputParameters)
    {
        connectionParameters.GetBlobContainerClient().GetDirectoryContents(inputParameters.SubFolder ?? string.Empty);
    }
}
