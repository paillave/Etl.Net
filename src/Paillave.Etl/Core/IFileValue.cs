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
        => (Stream, UnderlyingDisposables) = (stream, new ReadOnlyCollection<IDisposable>(underlyingDisposables));
    public Stream Stream { get; }
    public ReadOnlyCollection<IDisposable> UnderlyingDisposables { get; }

    public override bool CanRead => Stream.CanRead;

    public override bool CanSeek => Stream.CanSeek;

    public override bool CanWrite => Stream.CanWrite;

    public override long Length => Stream.Length;

    public override long Position { get => Stream.Position; set => Stream.Position = value; }

    public override void Flush()
    {
        Stream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return Stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return Stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        Stream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Stream.Write(buffer, offset, count);
    }
}