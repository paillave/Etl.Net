using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;

namespace Paillave.Etl.Core;

public class FileValueWriter : IFileValue
{
    private readonly StreamWriter _streamWriter;

    public FileValueWriter(string name, Encoding? encoding = null, int bufferSize = -1)
        => (_streamWriter, Name) = (new StreamWriter(new MemoryStream(), encoding, bufferSize, true), name);
    public FileValueWriter Write(string format, params object[] arg)
    {
        _streamWriter.Write(format, arg);
        return this;
    }
    public FileValueWriter Write(string format, object arg0, object arg1, object arg2)
    {
        _streamWriter.Write(format, arg0, arg1, arg2);
        return this;
    }
    public FileValueWriter Write(string format, object arg0)
    {
        _streamWriter.Write(format, arg0);
        return this;
    }
    public FileValueWriter Write(ReadOnlySpan<char> buffer)
    {
        _streamWriter.Write(buffer);
        return this;
    }
    public FileValueWriter Write(char[] buffer, int index, int count)
    {
        _streamWriter.Write(buffer, index, count);
        return this;
    }
    public FileValueWriter Write(char[] buffer)
    {
        _streamWriter.Write(buffer);
        return this;
    }
    public FileValueWriter Write(char value)
    {
        _streamWriter.Write(value);
        return this;
    }
    public FileValueWriter Write(string value)
    {
        _streamWriter.Write(value);
        return this;
    }
    public FileValueWriter WriteLine(string format, params object[] arg)
    {
        _streamWriter.WriteLine(format, arg);
        return this;
    }
    public FileValueWriter WriteLine(string format, object arg0, object arg1, object arg2)
    {
        _streamWriter.WriteLine(format, arg0, arg1, arg2);
        return this;
    }
    public FileValueWriter WriteLine(string format, object arg0, object arg1)
    {
        _streamWriter.WriteLine(format, arg0, arg1);
        return this;
    }
    public FileValueWriter WriteLine(string value)
    {
        _streamWriter.WriteLine(value);
        return this;
    }
    public FileValueWriter WriteLine(ReadOnlySpan<char> buffer)
    {
        _streamWriter.WriteLine(buffer);
        return this;
    }
    public FileValueWriter WriteLine(string format, object arg0)
    {
        _streamWriter.WriteLine(format, arg0);
        return this;
    }


    public Dictionary<string, IEnumerable<Destination>>? Destinations { get; set; }
    public string Name { get; }
    public string? ConnectorCode => null;

    public JsonNode? Metadata { get; set; }

    public void Delete() { }
    public Stream GetContent()
    {
        this._streamWriter.Flush();
        MemoryStream ms = new();
        this._streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
        this._streamWriter.BaseStream.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
    public StreamWithResource OpenContent()
    {
        this._streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
        return new StreamWithResource(_streamWriter.BaseStream);
    }
    public StreamWithResource Get(bool useStreamCopy = true)
    {
        if (!useStreamCopy)
            return OpenContent();

        var stream = new StreamWithResource(GetContent());
        stream.Position = 0;
        return stream;
    }

    public object Serialize() => throw new NotSupportedException("Serialization is not supported for FileValueWriter");

    public string GetSerialization()
    {
        this._streamWriter.Flush();
        var ms = new MemoryStream();
        this._streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
        this._streamWriter.BaseStream.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return Convert.ToBase64String(ms.ToArray());
    }
    public static FileValueWriter Create(string name, Encoding encoding = null, int bufferSize = -1)
        => new(name, encoding, bufferSize);
}