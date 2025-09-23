using System.IO;
namespace Paillave.Etl.Core;

public class InMemoryFileValue : FileValueBase
{
    private readonly Stream _stream;
    public InMemoryFileValue(Stream stream, string name) => (_stream, Name) = (stream, name);
    public override string Name { get; }
    public override Stream GetContent()
    {
        var ms = new MemoryStream();
        _stream.Seek(0, SeekOrigin.Begin);
        _stream.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public override StreamWithResource OpenContent() => new StreamWithResource(_stream);

    protected override void DeleteFile() { }
    public static IFileValue Create(Stream stream, string name)
        => new InMemoryFileValue(stream, name);
}
