using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Paillave.Etl.Core;
using Paillave.Etl.Zip;
using Xunit;

namespace Paillave.Etl.Zip.Tests;

public class ZipFileProcessorValuesProviderTests
{
    [Fact]
    public void DefaultFileName_DerivedFromInputName()
    {
        var input = new FakeFileValue("report.xml", "content"u8.ToArray());
        var provider = new ZipFileProcessorValuesProvider(new ZipFileProcessorParams());

        var result = PushOne(provider, input);

        Assert.Equal("report.zip", result.Name);
    }

    [Fact]
    public void CustomFileName_UsesSpecifiedName()
    {
        var input = new FakeFileValue("report.xml", "content"u8.ToArray());
        var provider = new ZipFileProcessorValuesProvider(new ZipFileProcessorParams { FileName = "MyReport" });

        var result = PushOne(provider, input);

        Assert.Equal("MyReport.zip", result.Name);
    }

    [Fact]
    public void CustomFileName_WithExtension_ExtensionReplacedWithZip()
    {
        var input = new FakeFileValue("report.xml", "content"u8.ToArray());
        var provider = new ZipFileProcessorValuesProvider(new ZipFileProcessorParams { FileName = "MyReport.xml" });

        var result = PushOne(provider, input);

        Assert.Equal("MyReport.zip", result.Name);
    }

    [Fact]
    public void EntryName_MatchesInputFileName()
    {
        var input = new FakeFileValue("data.xml", "content"u8.ToArray());
        var provider = new ZipFileProcessorValuesProvider(new ZipFileProcessorParams());

        var result = PushOne(provider, input);

        var entry = ReadFirstEntry(result, password: null);
        Assert.Equal("data.xml", entry.Name);
    }

    [Fact]
    public void Content_RoundTripsCorrectly()
    {
        var bytes = "<?xml version='1.0'?><Root/>"u8.ToArray();
        var input = new FakeFileValue("data.xml", bytes);
        var provider = new ZipFileProcessorValuesProvider(new ZipFileProcessorParams());

        var result = PushOne(provider, input);

        var actual = ReadFirstEntryContent(result, password: null);
        Assert.Equal(bytes, actual);
    }

    [Fact]
    public void Password_ContentReadableWithCorrectPassword()
    {
        var bytes = "secret"u8.ToArray();
        var input = new FakeFileValue("secret.xml", bytes);
        var provider = new ZipFileProcessorValuesProvider(new ZipFileProcessorParams { Password = "test1234" });

        var result = PushOne(provider, input);

        var actual = ReadFirstEntryContent(result, password: "test1234");
        Assert.Equal(bytes, actual);
    }

    [Fact]
    public void Password_ThrowsWithWrongPassword()
    {
        var bytes = "secret"u8.ToArray();
        var input = new FakeFileValue("secret.xml", bytes);
        var provider = new ZipFileProcessorValuesProvider(new ZipFileProcessorParams { Password = "test1234" });

        var result = PushOne(provider, input);

        Assert.Throws<ZipException>(() => ReadFirstEntryContent(result, password: "wrong"));
    }

    [Fact]
    public void GetContent_CanBeCalledMultipleTimes()
    {
        var bytes = "hello"u8.ToArray();
        var input = new FakeFileValue("a.xml", bytes);
        var provider = new ZipFileProcessorValuesProvider(new ZipFileProcessorParams());

        var result = PushOne(provider, input);

        var first = ReadFirstEntryContent(result, password: null);
        var second = ReadFirstEntryContent(result, password: null);
        Assert.Equal(first, second);
    }

    // --- helpers ---

    private static IFileValue PushOne(ZipFileProcessorValuesProvider provider, IFileValue input)
    {
        IFileValue? result = null;
        provider.PushValues(input, v => result = v, CancellationToken.None);
        Assert.NotNull(result);
        return result!;
    }

    private static ZipEntry ReadFirstEntry(IFileValue file, string? password)
    {
        using var stream = file.GetContent();
        using var zip = new ZipInputStream(stream);
        if (password != null) zip.Password = password;
        var entry = zip.GetNextEntry();
        Assert.NotNull(entry);
        return entry!;
    }

    private static byte[] ReadFirstEntryContent(IFileValue file, string? password)
    {
        using var stream = file.GetContent();
        using var zip = new ZipInputStream(stream);
        if (password != null) zip.Password = password;
        zip.GetNextEntry();
        var ms = new MemoryStream();
        zip.CopyTo(ms);
        return ms.ToArray();
    }
}

internal class FakeFileValue(string name, byte[] content) : IFileValue
{
    public string Name => name;
    public Stream GetContent() => new MemoryStream(content);
    public StreamWithResource Get(bool useStreamCopy = true) => new(new MemoryStream(content));
    public StreamWithResource OpenContent() => new(new MemoryStream(content));
    public void Delete() { }
    public JsonNode? Metadata { get; set; }
    public Dictionary<string, IEnumerable<Destination>>? Destinations { get; set; }
}
