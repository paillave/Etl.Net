namespace Paillave.Etl.AzureStorageAccountFileProvider;

public class AzureBlobOptions
{
    public Uri? BaseUri { get; set; }
    public bool? DefaultAzureCredential { get; set; }
    public required string DocumentContainer { get; set; }
    public string? ConnectionString { get; set; }
}
