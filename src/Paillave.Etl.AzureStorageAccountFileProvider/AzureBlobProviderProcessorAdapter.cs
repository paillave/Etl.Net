using Paillave.Etl.Core;

namespace Paillave.Etl.AzureStorageAccountFileProvider;

public class AzureStorageAccountProviderProcessorAdapter : ProviderProcessorAdapterBase<AzureBlobOptions,
                                                                             AzureStorageAccountAdapterProviderParameters,
                                                                             AzureStorageAccountAdapterProcessorParameters>
{
    public override string Description => "Get file and create document in azure storage account";
    public override string Name => "AzureStorageAccount";

    protected override IFileValueProvider CreateProvider(string code, string name, string connectionName,
                                                         AzureBlobOptions connectionParameters,
                                                         AzureStorageAccountAdapterProviderParameters inputParameters)
        => new AzureStorageAccountFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);

    protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName,
                                                           AzureBlobOptions connectionParameters,
                                                           AzureStorageAccountAdapterProcessorParameters outputParameters)
        => new AzureStorageAccountFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
}
