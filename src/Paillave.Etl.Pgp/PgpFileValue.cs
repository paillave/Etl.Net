using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Pgp;

public class PgpFileValue : FileValueBase
{
    private readonly Stream _stream;
    private readonly IFileValue _underlyingFileValue;
    public override string Name { get; }
    public PgpFileValue(Stream stream, string name, IFileValue underlyingFileValue)
        => (_stream, Name, _underlyingFileValue)
        = (stream, name, underlyingFileValue);
    public override Stream GetContent()
    {
        var ms = new MemoryStream();
        _stream.Seek(0, SeekOrigin.Begin);
        _stream.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
    protected override void DeleteFile()
    {
        _underlyingFileValue.Delete();
    }

    public override StreamWithResource OpenContent()
    {
        _stream.Seek(0, SeekOrigin.Begin);
        return new StreamWithResource(_stream);
    }
}