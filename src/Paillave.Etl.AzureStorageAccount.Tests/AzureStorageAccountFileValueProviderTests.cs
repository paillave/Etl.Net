using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using Azure.Storage.Blobs;
using Paillave.Etl.Core;
using Xunit;

namespace Paillave.Etl.AzureStorageAccount.Tests;

// A FileReference emitted while listing must be enough to get the same blob back through
// Provide(fileSpecific) - that is how event-driven consumers (connector pollers, workflows) reopen
// a listed file. Blobs under a SubFolder must round-trip, not only blobs at the container root.
// Requires Azurite on 127.0.0.1:10000 (docker run -p 10000:10000 mcr.microsoft.com/azure-storage/azurite azurite-blob --blobHost 0.0.0.0 --skipApiVersionCheck); skipped otherwise.
public sealed class AzureStorageAccountFileValueProviderTests : IDisposable
{
    private const string ConnectionString = "UseDevelopmentStorage=true";
    private const string FileName = "statement.json";
    private const string FileContent = "{\"data\":[]}";

    private readonly string containerName = $"etl-test-{Guid.NewGuid():N}";
    private readonly BlobContainerClient container;

    public AzureStorageAccountFileValueProviderTests()
    {
        container = new BlobContainerClient(ConnectionString, containerName);
        container.CreateIfNotExists();
    }

    public void Dispose() => container.DeleteIfExists();

    [AzuriteFact]
    public void SubFolderBlob_FileSpecificHoldsFullBlobPath()
    {
        var provider = CreateProvider("bank/ToProcess/");
        Upload($"bank/ToProcess/{FileName}");

        var fileReference = ListSingle(provider);

        Assert.Equal(FileName, fileReference.Name);
        Assert.Equal($"bank/ToProcess/{FileName}", fileReference.FileSpecific["Folder"]!.GetValue<string>());
    }

    [AzuriteFact]
    public void SubFolderBlob_ContentCanBeReadBackFromFileSpecific()
    {
        var provider = CreateProvider("bank/ToProcess/");
        Upload($"bank/ToProcess/{FileName}");

        var fileValue = provider.Provide(Reparse(ListSingle(provider).FileSpecific));

        Assert.Equal(FileContent, ReadContent(fileValue));
    }

    [AzuriteFact]
    public void SubFolderBlob_DeleteFromFileSpecificRemovesTheListedBlob()
    {
        var provider = CreateProvider("bank/ToProcess/");
        Upload($"bank/ToProcess/{FileName}");

        provider.Provide(Reparse(ListSingle(provider).FileSpecific)).Delete();

        Assert.False(container.GetBlobClient($"bank/ToProcess/{FileName}").Exists().Value);
    }

    [AzuriteFact]
    public void RootBlob_ContentCanBeReadBackFromFileSpecific()
    {
        var provider = CreateProvider(null);
        Upload(FileName);

        var fileValue = provider.Provide(Reparse(ListSingle(provider).FileSpecific));

        Assert.Equal(FileContent, ReadContent(fileValue));
    }

    [AzuriteFact]
    public void SubFolderWithoutTrailingSlash_ListsFilesOfThatFolderOnly()
    {
        var provider = CreateProvider("bank/ToProcess");
        Upload($"bank/ToProcess/{FileName}");
        Upload("bank/ToProcessOld/other.json");

        Assert.Equal([FileName], ListNames(provider));
    }

    [AzuriteFact]
    public void SubFolder_DoesNotListBlobsOfNestedFolders()
    {
        var provider = CreateProvider("bank/ToProcess/");
        Upload($"bank/ToProcess/{FileName}");
        Upload("bank/ToProcess/Archived/old.json");

        Assert.Equal([FileName], ListNames(provider));
    }

    [AzuriteFact]
    public void FileNamePattern_SplitsProvidersSharingOneFolder()
    {
        var movements = CreateProvider("bank/ToProcess/", "MOVEMENTS_*.TXT");
        var balances = CreateProvider("bank/ToProcess/", "BALANCES_*.TXT");
        Upload("bank/ToProcess/MOVEMENTS_20260924.TXT");
        Upload("bank/ToProcess/BALANCES_20260924.TXT");

        Assert.Equal(["MOVEMENTS_20260924.TXT"], ListNames(movements));
        Assert.Equal(["BALANCES_20260924.TXT"], ListNames(balances));
    }

    [AzuriteFact]
    public void FileNamePattern_IgnoresCase()
    {
        var provider = CreateProvider("bank/ToProcess/", "MOVEMENTS_*.TXT");
        Upload("bank/ToProcess/movements_20260924.txt");

        Assert.Equal(["movements_20260924.txt"], ListNames(provider));
    }

    private AzureStorageAccountFileValueProvider CreateProvider(string? subFolder, string? fileNamePattern = null) =>
        new("BankToProcess", "BankToProcess", "AzureStorageAccount",
            new AzureBlobOptions { ConnectionString = ConnectionString, DocumentContainer = containerName },
            new AzureStorageAccountAdapterProviderParameters { SubFolder = subFolder, FileNamePattern = fileNamePattern });

    private static List<string> ListNames(IFileValueProvider provider)
    {
        var names = new List<string>();
        provider.Provide(new object(), (_, fileReference) => names.Add(fileReference.Name), CancellationToken.None);
        names.Sort(StringComparer.Ordinal);
        return names;
    }

    private void Upload(string blobPath) =>
        container.GetBlobClient(blobPath).Upload(new MemoryStream(Encoding.UTF8.GetBytes(FileContent)), overwrite: true);

    private static FileReference ListSingle(IFileValueProvider provider)
    {
        var references = new List<FileReference>();
        provider.Provide(new object(), (_, fileReference) => references.Add(fileReference), CancellationToken.None);
        return Assert.Single(references);
    }

    // fileSpecific is persisted as JSON between listing and reopening; reparse to drop any in-memory state.
    private static JsonNode Reparse(JsonNode fileSpecific) => JsonNode.Parse(fileSpecific.ToJsonString())!;

    private static string ReadContent(IFileValue fileValue)
    {
        using var reader = new StreamReader(fileValue.GetContent());
        return reader.ReadToEnd();
    }
}

public sealed class AzuriteFactAttribute : FactAttribute
{
    public AzuriteFactAttribute()
    {
        if (!IsAzuriteRunning())
            Skip = "Azurite blob emulator not reachable on 127.0.0.1:10000";
    }

    private static bool IsAzuriteRunning()
    {
        try
        {
            using var client = new TcpClient();
            return client.ConnectAsync("127.0.0.1", 10000).Wait(TimeSpan.FromMilliseconds(500)) && client.Connected;
        }
        catch
        {
            return false;
        }
    }
}
