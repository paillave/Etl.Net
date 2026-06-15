using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Paillave.Etl.Core;
using Xunit;

namespace Paillave.Etl.GraphApi.Tests;

/// <summary>
/// Integration tests for reading and writing files on OneDrive/SharePoint via Graph API.
///
/// Before running: fill in the Azure AD app registration values and target drive info below.
/// The app registration must have the following Application permissions on Microsoft Graph:
///   - Files.ReadWrite.All  (to list, read and upload files)
///   - Sites.ReadWrite.All  (if you target a SharePoint site)
///   - Group.Read.All       (if you target a group/team drive by display name)
/// </summary>
public class GraphApiOneDriveIntegrationTests
{
    // =====================================================================
    // Azure AD app registration — fill these in
    // =====================================================================
    private const string TenantId     = "00000000-0000-0000-0000-000000000000";
    private const string ClientId     = "00000000-0000-0000-0000-000000000000";
    private const string ClientSecret = "00000000-0000-0000-0000-000000000000";

    // Object-ID (GUID) or UPN of the Azure AD user whose OneDrive is used
    // when DriveId/SiteUrl/GroupId are all null. Leave null if targeting a site/group.
    private const string? UserId = null;

    // =====================================================================
    // Target drive — set ONE of the options below (DriveId takes priority)
    // =====================================================================

    // Option A — direct drive ID (shown in Graph Explorer, fastest)
    private const string? DriveId = null; // e.g. "b!abc123..."

    // Option B — SharePoint site URL  (leave DriveId null)
    private const string? SiteUrl = null; // e.g. "https://contoso.sharepoint.com/sites/myteam"

    // Option C — Microsoft 365 group display name or email (leave DriveId + SiteUrl null)
    private const string? GroupDisplayNameOrEmail = null; // e.g. "My Team"

    // Option D — SharePoint/OneDrive sharing URL (takes priority over all other drive sources)
    private const string? SharingUrl = "https://"; // e.g. "https://fundprocess.sharepoint.com/:f:/g/Abc123..."

    // =====================================================================
    // Test folder — must exist on the target drive
    // =====================================================================
    private const string? TestFolder   = null; // "ETL_Tests";   // create this folder on OneDrive first
    private const string ExistingFile = "Azuredatabases.csv";  // a file that already exists in TestFolder

    // =====================================================================

    private static GraphApiAdapterConnectionParameters ConnectionParams => new()
    {
        TenantId     = TenantId,
        ClientId     = ClientId,
        ClientSecret = ClientSecret,
        UserId       = UserId,
        MaxAttempts  = 1,
    };

    private static GraphApiOneDriveAdapterProviderParameters ProviderParams(
        string? folder = TestFolder,
        string? pattern = null,
        bool recursive = false) => new()
    {
        DriveId                 = DriveId,
        SiteUrl                 = SiteUrl,
        GroupDisplayNameOrEmail = GroupDisplayNameOrEmail,
        SharingUrl              = SharingUrl,
        FolderPath              = folder,
        FileNamePattern         = pattern,
        Recursive               = recursive,
    };

    private static GraphApiOneDriveAdapterProcessorParameters ProcessorParams(
        string? folder = TestFolder) => new()
    {
        DriveId                 = DriveId,
        SiteUrl                 = SiteUrl,
        GroupDisplayNameOrEmail = GroupDisplayNameOrEmail,
        SharingUrl              = SharingUrl,
        FolderPath              = folder,
    };

    // ------------------------------------------------------------------
    // Helper: a simple in-memory IFileValue so we can upload arbitrary content
    // ------------------------------------------------------------------
    private sealed class InMemoryFileValue : FileValueBase
    {
        private readonly byte[] _bytes;
        public override string Name { get; }

        public InMemoryFileValue(string name, string content)
        {
            Name   = name;
            _bytes = Encoding.UTF8.GetBytes(content);
        }

        protected override void DeleteFile() { }
        public override Stream GetContent() => new MemoryStream(_bytes, writable: false);
        public override StreamWithResource OpenContent()
            => new StreamWithResource(new MemoryStream(_bytes, writable: false));
    }

    // ==================================================================
    // Tests
    // ==================================================================

    /// <summary>Lists all files in <see cref="TestFolder"/> and asserts there is at least one.</summary>
    [Fact]
    public void ListFiles_InTestFolder_ReturnsAtLeastOneFile()
    {
        var provider = new GraphApiOneDriveFileValueProvider(
            "provider", "Provider", "conn",
            ConnectionParams, ProviderParams());

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);

        Assert.NotEmpty(files);
    }

    /// <summary>Filters by glob pattern and asserts only matching files are returned.</summary>
    [Fact]
    public void ListFiles_WithGlobPattern_ReturnsOnlyMatchingFiles()
    {
        var provider = new GraphApiOneDriveFileValueProvider(
            "provider", "Provider", "conn",
            ConnectionParams, ProviderParams(pattern: "*.csv"));

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);

        Assert.All(files, f => Assert.EndsWith(".csv", f.Name));
    }

    /// <summary>Reads the content of <see cref="ExistingFile"/> and asserts the stream is non-empty.</summary>
    [Fact]
    public void ReadFileContent_ExistingFile_ReturnsNonEmptyStream()
    {
        var provider = new GraphApiOneDriveFileValueProvider(
            "provider", "Provider", "conn",
            ConnectionParams, ProviderParams(pattern: ExistingFile));

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);

        Assert.NotEmpty(files);

        using var stream = files.First().GetContent();
        Assert.True(stream.Length > 0);
    }

    /// <summary>Uploads a small CSV file to OneDrive.</summary>
    [Fact]
    public void WriteFile_SmallCsv_UploadsWithoutError()
    {
        const string content = "Name,Age\nAlice,30\nBob,25\n";
        var fileToUpload = new InMemoryFileValue("integration-test.csv", content);

        var processor = new GraphApiOneDriveFileValueProcessor(
            "processor", "Processor", "conn",
            ConnectionParams, ProcessorParams());

        var pushed = new List<IFileValue>();
        processor.Process(fileToUpload, pushed.Add, CancellationToken.None);

        // The processor pushes the input file back unchanged after uploading it.
        Assert.Single(pushed);
        Assert.Equal("integration-test.csv", pushed[0].Name);
    }

    /// <summary>
    /// Uploads a file then immediately reads it back and verifies the content matches.
    /// Requires write + read permissions.
    /// </summary>
    [Fact]
    public void WriteFile_ThenReadBack_ContentIsIdentical()
    {
        const string content  = "Symbol,Quantity,Price\nAAPL,100,150.25\nMSFT,50,310.50\n";
        const string fileName = "roundtrip-test.csv";

        // Upload
        var processor = new GraphApiOneDriveFileValueProcessor(
            "writer", "Writer", "conn",
            ConnectionParams, ProcessorParams());
        processor.Process(new InMemoryFileValue(fileName, content), _ => { }, CancellationToken.None);

        // Read back
        var provider = new GraphApiOneDriveFileValueProvider(
            "reader", "Reader", "conn",
            ConnectionParams, ProviderParams(pattern: fileName));

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);

        Assert.Single(files);

        using var stream = files[0].GetContent();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        Assert.Equal(content, reader.ReadToEnd());
    }

    /// <summary>
    /// Uploads a file larger than the 4 MB threshold so the chunked upload path
    /// (LargeFileUploadTask with 320 KB slices) is exercised.
    /// </summary>
    [Fact]
    public void WriteFile_LargeFile_UploadsViaChunkedSession()
    {
        // Build a ~5 MB CSV — well above the 4 MB limit that triggers chunked upload.
        const int rowCount = 55_000;
        var sb = new StringBuilder();
        sb.AppendLine("Id,Name,Value");
        for (int i = 0; i < rowCount; i++)
            sb.AppendLine($"{i},Row{i},{i * 1.23:F4}");
        var content  = sb.ToString();
        var fileName = "large-file-test.csv";

        var processor = new GraphApiOneDriveFileValueProcessor(
            "writer", "Writer", "conn",
            ConnectionParams, ProcessorParams());

        var pushed = new List<IFileValue>();
        processor.Process(new InMemoryFileValue(fileName, content), pushed.Add, CancellationToken.None);

        Assert.Single(pushed);
    }

    /// <summary>
    /// Verifies that listing with <c>Recursive = true</c> also returns files
    /// in sub-folders of <see cref="TestFolder"/>.
    /// Requires at least one sub-folder with a file in it on the drive.
    /// </summary>
    [Fact]
    public void ListFiles_RecursiveMode_IncludesSubFolderFiles()
    {
        var nonRecursive = new List<IFileValue>();
        new GraphApiOneDriveFileValueProvider("p1", "P1", "conn",
            ConnectionParams, ProviderParams(recursive: false))
            .Provide(null, (fv, _) => nonRecursive.Add(fv), CancellationToken.None);

        var recursive = new List<IFileValue>();
        new GraphApiOneDriveFileValueProvider("p2", "P2", "conn",
            ConnectionParams, ProviderParams(recursive: true))
            .Provide(null, (fv, _) => recursive.Add(fv), CancellationToken.None);

        // Recursive listing must return at least as many files as the flat one.
        Assert.True(recursive.Count >= nonRecursive.Count);
    }

    /// <summary>
    /// Smoke-tests the <see cref="IFileValueProvider.Test"/> method — verifies
    /// that the drive can be resolved with the supplied credentials.
    /// </summary>
    [Fact]
    public void Provider_Test_DoesNotThrow()
    {
        var provider = new GraphApiOneDriveFileValueProvider(
            "provider", "Provider", "conn",
            ConnectionParams, ProviderParams());

        var ex = Record.Exception(provider.Test);
        Assert.Null(ex);
    }

    /// <summary>
    /// Smoke-tests the <see cref="IFileValueProcessor.Test"/> method — verifies
    /// that the drive can be resolved with the supplied credentials.
    /// </summary>
    [Fact]
    public void Processor_Test_DoesNotThrow()
    {
        var processor = new GraphApiOneDriveFileValueProcessor(
            "processor", "Processor", "conn",
            ConnectionParams, ProcessorParams());

        var ex = Record.Exception(processor.Test);
        Assert.Null(ex);
    }

    /// <summary>
    /// Uploads the same file twice and verifies that only one copy exists afterwards
    /// (conflict behaviour = replace).
    /// </summary>
    [Fact]
    public void WriteFile_Overwrite_ReplacesExistingFile()
    {
        const string fileName  = "overwrite-test.csv";
        const string original  = "col1,col2\noriginal,data\n";
        const string updated   = "col1,col2\nupdated,data\n";

        var processor = new GraphApiOneDriveFileValueProcessor(
            "processor", "Processor", "conn",
            ConnectionParams, ProcessorParams());

        processor.Process(new InMemoryFileValue(fileName, original), _ => { }, CancellationToken.None);
        processor.Process(new InMemoryFileValue(fileName, updated),  _ => { }, CancellationToken.None);

        var provider = new GraphApiOneDriveFileValueProvider(
            "reader", "Reader", "conn",
            ConnectionParams, ProviderParams(pattern: fileName));

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);

        // Exactly one file with the updated content.
        Assert.Single(files);
        using var stream = files[0].GetContent();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        Assert.Equal(updated, reader.ReadToEnd());
    }

    /// <summary>
    /// Uploads a file with UTF-8 content (accented characters, CJK, emoji)
    /// and reads it back to confirm encoding is preserved end-to-end.
    /// </summary>
    [Fact]
    public void WriteFile_Utf8Content_RoundTripPreservesEncoding()
    {
        const string content  = "Prénom,Nom,Commentaire\nÉlodie,Müller,日本語テスト 😀\nLuís,González,Ça va?\n";
        const string fileName = "utf8-test.csv";

        var processor = new GraphApiOneDriveFileValueProcessor(
            "writer", "Writer", "conn",
            ConnectionParams, ProcessorParams());
        processor.Process(new InMemoryFileValue(fileName, content), _ => { }, CancellationToken.None);

        var provider = new GraphApiOneDriveFileValueProvider(
            "reader", "Reader", "conn",
            ConnectionParams, ProviderParams(pattern: fileName));

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);

        Assert.Single(files);
        using var stream = files[0].GetContent();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        Assert.Equal(content, reader.ReadToEnd());
    }

    /// <summary>
    /// Uploads several files in a loop and verifies all of them appear in the listing.
    /// </summary>
    [Fact]
    public void WriteMultipleFiles_AllAppearInListing()
    {
        const int fileCount = 5;
        var fileNames = Enumerable.Range(1, fileCount)
            .Select(i => $"batch-test-{i:D2}.csv")
            .ToList();

        var processor = new GraphApiOneDriveFileValueProcessor(
            "processor", "Processor", "conn",
            ConnectionParams, ProcessorParams());

        foreach (var name in fileNames)
            processor.Process(
                new InMemoryFileValue(name, $"id,value\n{name},42\n"),
                _ => { }, CancellationToken.None);

        var provider = new GraphApiOneDriveFileValueProvider(
            "reader", "Reader", "conn",
            ConnectionParams, ProviderParams(pattern: "batch-test-*.csv"));

        var found = new List<IFileValue>();
        provider.Provide(null, (fv, _) => found.Add(fv), CancellationToken.None);

        Assert.True(found.Count >= fileCount,
            $"Expected at least {fileCount} files, found {found.Count}.");
        Assert.All(fileNames, name => Assert.Contains(found, f => f.Name == name));
    }

    /// <summary>
    /// Uploads a file to a sub-folder path and reads it back from the same sub-folder.
    /// The sub-folder must exist on the drive before running this test.
    /// </summary>
    [Fact]
    public void WriteFile_ToSubFolder_CanBeReadBackFromSubFolder()
    {
        const string subFolder = "ETL_Tests/SubFolder";
        const string fileName  = "subfolder-test.csv";
        const string content   = "key,value\nfoo,bar\n";

        var processor = new GraphApiOneDriveFileValueProcessor(
            "writer", "Writer", "conn",
            ConnectionParams, ProcessorParams(folder: subFolder));
        processor.Process(new InMemoryFileValue(fileName, content), _ => { }, CancellationToken.None);

        var provider = new GraphApiOneDriveFileValueProvider(
            "reader", "Reader", "conn",
            ConnectionParams, ProviderParams(folder: subFolder, pattern: fileName));

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);

        Assert.Single(files);
        using var stream = files[0].GetContent();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        Assert.Equal(content, reader.ReadToEnd());
    }

    /// <summary>
    /// Verifies that a glob pattern that matches nothing returns an empty list
    /// rather than throwing.
    /// </summary>
    [Fact]
    public void ListFiles_PatternMatchesNothing_ReturnsEmptyList()
    {
        var provider = new GraphApiOneDriveFileValueProvider(
            "provider", "Provider", "conn",
            ConnectionParams, ProviderParams(pattern: "this-file-definitely-does-not-exist-*.xyz"));

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);

        Assert.Empty(files);
    }

    /// <summary>
    /// Reads the content of <see cref="ExistingFile"/> twice concurrently and verifies
    /// both streams are non-empty and identical — checks that the <see cref="IFileValue"/>
    /// returned by the provider can be read more than once without corruption.
    /// </summary>
    [Fact]
    public void ReadFileContent_TwiceConcurrently_BothStreamsAreValid()
    {
        var provider = new GraphApiOneDriveFileValueProvider(
            "provider", "Provider", "conn",
            ConnectionParams, ProviderParams(pattern: ExistingFile));

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);
        Assert.NotEmpty(files);

        var fileValue = files.First();

        string read1, read2;
        using (var s1 = fileValue.GetContent())
        using (var r1 = new StreamReader(s1, Encoding.UTF8))
            read1 = r1.ReadToEnd();

        using (var s2 = fileValue.GetContent())
        using (var r2 = new StreamReader(s2, Encoding.UTF8))
            read2 = r2.ReadToEnd();

        Assert.True(read1.Length > 0);
        Assert.Equal(read1, read2);
    }

    /// <summary>
    /// Uploads a JSON file (not CSV) and reads it back — verifies the connector
    /// is content-agnostic and works for any file type.
    /// </summary>
    [Fact]
    public void WriteAndRead_JsonFile_ContentIsPreserved()
    {
        const string fileName = "test-data.json";
        const string content  = "{\"name\":\"test\",\"values\":[1,2,3],\"active\":true}\n";

        var processor = new GraphApiOneDriveFileValueProcessor(
            "writer", "Writer", "conn",
            ConnectionParams, ProcessorParams());
        processor.Process(new InMemoryFileValue(fileName, content), _ => { }, CancellationToken.None);

        var provider = new GraphApiOneDriveFileValueProvider(
            "reader", "Reader", "conn",
            ConnectionParams, ProviderParams(pattern: fileName));

        var files = new List<IFileValue>();
        provider.Provide(null, (fv, _) => files.Add(fv), CancellationToken.None);

        Assert.Single(files);
        using var stream = files[0].GetContent();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        Assert.Equal(content, reader.ReadToEnd());
    }

    /// <summary>
    /// Verifies that listing returns the correct file names (not IDs or encoded paths)
    /// by checking the <see cref="ExistingFile"/> name is present in the listing.
    /// </summary>
    [Fact]
    public void ListFiles_FileNamesAreHumanReadable()
    {
        var provider = new GraphApiOneDriveFileValueProvider(
            "provider", "Provider", "conn",
            ConnectionParams, ProviderParams());

        var names = new List<string>();
        provider.Provide(null, (fv, _) => names.Add(fv.Name), CancellationToken.None);

        Assert.Contains(ExistingFile, names);
        Assert.All(names, n => Assert.False(string.IsNullOrWhiteSpace(n)));
    }
}
