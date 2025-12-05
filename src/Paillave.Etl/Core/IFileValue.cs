using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace Paillave.Etl.Core;

public interface IFileValue
{
    string Name { get; }
    StreamWithResource Get(bool useStreamCopy = true);
    Stream GetContent();
    StreamWithResource OpenContent();
    void Delete();
    JsonNode? Metadata { get; set; }
    Dictionary<string, IEnumerable<Destination>>? Destinations { get; set; }
}
public class StreamWithResource(Stream stream, params IDisposable[] underlyingDisposables) : Stream
{
    public override bool CanRead => stream.CanRead;
    public override bool CanSeek => stream.CanSeek;
    public override bool CanWrite => stream.CanWrite;
    public override long Length => stream.Length;
    public override long Position { get => stream.Position; set => stream.Position = value; }
    public override void Flush() => stream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
    public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
    public override void SetLength(long value) => stream.SetLength(value);
    public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);
    protected override void Dispose(bool disposing)
    {
        if (disposing)
            foreach (var item in underlyingDisposables)
                item.Dispose();
        base.Dispose(disposing);
    }
}

public class Destination
{
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string StreetAddress { get; set; }
    public string ZipCode { get; set; }
    public string Location { get; set; }
    public string Country { get; set; }
    public string Culture { get; set; }
}
