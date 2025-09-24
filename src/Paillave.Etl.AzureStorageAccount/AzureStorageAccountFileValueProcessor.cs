using Paillave.Etl.Core;

namespace Paillave.Etl.AzureStorageAccount;

public class AzureStorageAccountAdapterProcessorParameters
{
    public string? SubFolder { get; set; }
    public bool? OverwriteIfAlreadyExists { get; set; } = false;
}

public class AzureStorageAccountFileValueProcessor(string code, string name, string connectionName, AzureBlobOptions connectionParameters, AzureStorageAccountAdapterProcessorParameters processorParameters) 
    : FileValueProcessorBase<AzureBlobOptions, AzureStorageAccountAdapterProcessorParameters>(code, name, connectionName, connectionParameters, processorParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override void Process(IFileValue fileValue, AzureBlobOptions connectionParameters,
        AzureStorageAccountAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken)
    {
        IDictionary<string, string>? metadata = ExtractMetadataRecursively(fileValue.Metadata);
        var blobContainerClient = connectionParameters.GetBlobContainerClient();
        var subpath = string.IsNullOrWhiteSpace(processorParameters.SubFolder)
            ? fileValue.Name
            : $"{processorParameters.SubFolder.TrimEnd('/')}/{fileValue.Name}";
        blobContainerClient.SaveFileAsync(
            subpath,
            fileValue.GetContent(),
            processorParameters.OverwriteIfAlreadyExists ?? false,
            metadata,
            cancellationToken).Wait(cancellationToken);
        push(fileValue);
    }

    private static IDictionary<string, string>? ExtractMetadataRecursively(object? metadata)
    {
        if (metadata == null)
            return null;
        var result = new Dictionary<string, string>();
        if (metadata != null)
        {
            foreach (var property in metadata.GetType().GetProperties())
            {
                var value = property.GetValue(metadata);
                if (value != null)
                {
                    var stringValue = value.ToString();
                    if (!string.IsNullOrWhiteSpace(stringValue))
                        result[property.Name] = stringValue;
                }
            }
        }
        return result;
    }

    protected override void Test(AzureBlobOptions connectionParameters, AzureStorageAccountAdapterProcessorParameters processorParameters)
    {
        var blobContainerClient = connectionParameters.GetBlobContainerClient();
        var fileValueName = Guid.NewGuid().ToString();
        var subPath = string.IsNullOrWhiteSpace(processorParameters.SubFolder) ? fileValueName : $"{processorParameters.SubFolder.TrimEnd('/')}/{fileValueName}";
        var ms = new MemoryStream();
        blobContainerClient.SaveFileAsync(subPath, ms).Wait();
        blobContainerClient.GetBlobClient(subPath).Delete();
    }
}
