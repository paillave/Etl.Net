using Paillave.Etl.Core;

namespace Paillave.Etl.AzureStorageAccount;

public class AzureBlobOptions
{
    public Uri? BaseUri { get; set; }
    public bool? DefaultAzureCredential { get; set; }
    public required string DocumentContainer { get; set; }
    [Sensitive]
    public string? ConnectionString { get; set; }
}
