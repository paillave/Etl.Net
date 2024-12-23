using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;

namespace Paillave.Etl.AzureStorageAccountFileProvider;

public class AzureStorageAccountAdapterProcessorParameters
{
    public string? SubFolder { get; set; }
    public bool? OverwriteIfAlreadyExists { get; set; } = false;
}

public class AzureStorageAccountFileValueProcessor : FileValueProcessorBase<AzureBlobOptions, AzureStorageAccountAdapterProcessorParameters>
{
    public AzureStorageAccountFileValueProcessor(string code, string name, string connectionName, AzureBlobOptions connectionParameters, AzureStorageAccountAdapterProcessorParameters processorParameters)
        : base(code, name, connectionName, connectionParameters, processorParameters) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override void Process(IFileValue fileValue, AzureBlobOptions connectionParameters, AzureStorageAccountAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
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
            cancellationToken).Wait();
        push(fileValue);
    }

    private IDictionary<string, string>? ExtractMetadataRecursively(IFileValueMetadata metadata)
    {
        if (metadata == null)
            return null;
        var result = new Dictionary<string, string>();
        if (metadata.Properties != null)
        {
            foreach (var property in metadata.Properties.GetType().GetProperties())
            {
                var value = property.GetValue(metadata.Properties);
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

    private byte[] ToByteArray(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}

