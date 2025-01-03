using Microsoft.Extensions.FileSystemGlobbing;
using Paillave.Etl.Core;

namespace Paillave.Etl.AzureStorageAccountFileProvider;

public class AzureStorageAccountAdapterProviderParameters
{
    public string? SubFolder { get; set; }
    public string? FileNamePattern { get; set; }
}

public class AzureStorageAccountFileValueProvider : FileValueProviderBase<AzureBlobOptions, AzureStorageAccountAdapterProviderParameters>
{
    public AzureStorageAccountFileValueProvider(string code, string name, string connectionName,
                                     AzureBlobOptions connectionParameters,
                                     AzureStorageAccountAdapterProviderParameters inputParameters)
        : base(code, name, connectionName, connectionParameters, inputParameters) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override void Provide(Action<IFileValue> pushFileValue, AzureBlobOptions connectionParameters,
                                    AzureStorageAccountAdapterProviderParameters providerParameters, CancellationToken cancellationToken,
                                    IExecutionContext context)
    {
        var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        var matcher = new Matcher().AddInclude(searchPattern);
        var blobContainerClient = connectionParameters.GetBlobContainerClient();
        foreach (var blobHierarchyItem in blobContainerClient.GetDirectoryContents(providerParameters.SubFolder ?? string.Empty, cancellationToken).Cast<AzureBlobFileInfo>())
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (!blobHierarchyItem.IsDirectory)
            {
                if (matcher.Match(blobHierarchyItem.Name).HasMatches)
                    pushFileValue(new AzureStorageAccountFileValue(blobHierarchyItem, this.Code, this.Name, this.ConnectionName, connectionParameters));
            }
        }
    }

    protected override void Test(AzureBlobOptions connectionParameters, AzureStorageAccountAdapterProviderParameters inputParameters)
    {
        connectionParameters.GetBlobContainerClient().GetDirectoryContents(inputParameters.SubFolder ?? string.Empty);
    }
}
