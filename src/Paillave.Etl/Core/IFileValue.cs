using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Paillave.Etl.Core;
public interface IFileValue
{
    string Name { get; }
    StreamWithResource Get(bool useStreamCopy = true);
    Stream GetContent();
    StreamWithResource OpenContent();
    void Delete();
    Type MetadataType { get; }
    IFileValueMetadata Metadata { get; }
    string SourceType { get; }
}
public interface IFileValueMetadata
{
    string Type { get; }
    string? ConnectorCode { get; }
    string? ConnectionName { get; }
    string? ConnectorName { get; }
}
public class StreamWithResource : Stream
{
    public StreamWithResource(Stream stream, params IDisposable[] underlyingDisposables)
        => (_stream, _underlyingDisposables)
        = (stream, new ReadOnlyCollection<IDisposable>(underlyingDisposables));
    public readonly Stream _stream;
    public readonly ReadOnlyCollection<IDisposable> _underlyingDisposables;

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => _stream.CanWrite;
    public override long Length => _stream.Length;
    public override long Position { get => _stream.Position; set => _stream.Position = value; }
    public override void Flush() => _stream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
    public override void SetLength(long value) => _stream.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            foreach (var item in _underlyingDisposables)
                item.Dispose();
        base.Dispose(disposing);
    }
}
