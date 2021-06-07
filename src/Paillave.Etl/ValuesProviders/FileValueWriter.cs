using System;
using System.IO;
using System.Text;
using System.Text.Json;
namespace Paillave.Etl.ValuesProviders
{
    public class FileValueWriter<TMetadata> : IFileValue
        where TMetadata : IFileValueMetadata
    {
        private readonly StreamWriter _streamWriter;
        public FileValueWriter(TMetadata metadata, string name, Encoding encoding = null, int bufferSize = -1)
            => (_streamWriter, Name, Metadata) = (new StreamWriter(new MemoryStream(), encoding, bufferSize, true), name, metadata);

        public void Write(string format, params object[] arg) => _streamWriter.Write(format, arg);
        public void Write(string format, object arg0, object arg1, object arg2) => _streamWriter.Write(format, arg0, arg1, arg2);
        public void Write(string format, object arg0) => _streamWriter.Write(format, arg0);
        public void Write(ReadOnlySpan<char> buffer) => _streamWriter.Write(buffer);
        public void Write(char[] buffer, int index, int count) => _streamWriter.Write(buffer, index, count);
        public void Write(char[] buffer) => _streamWriter.Write(buffer);
        public void Write(char value) => _streamWriter.Write(value);
        public void Write(string value) => _streamWriter.Write(value);
        public void WriteLine(string format, params object[] arg) => _streamWriter.WriteLine(format, arg);
        public void WriteLine(string format, object arg0, object arg1, object arg2) => _streamWriter.WriteLine(format, arg0, arg1, arg2);
        public void WriteLine(string format, object arg0, object arg1) => _streamWriter.WriteLine(format, arg0, arg1);
        public void WriteLine(string value) => _streamWriter.WriteLine(value);
        public void WriteLine(ReadOnlySpan<char> buffer) => _streamWriter.WriteLine(buffer);
        public void WriteLine(string format, object arg0) => _streamWriter.WriteLine(format, arg0);


        public TMetadata Metadata { get; }
        public virtual string SourceType => this.Metadata.Type;
        public Type MetadataType => typeof(TMetadata);
        IFileValueMetadata IFileValue.Metadata => this.Metadata;
        public string Name { get; }
        public void Delete() { }
        public Stream GetContent()
        {
            this._streamWriter.Flush();
            MemoryStream ms = new MemoryStream();
            this._streamWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            this._streamWriter.BaseStream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
    public static class FileValueWriter
    {
        public static FileValueWriter<TMetadata> Create<TMetadata>(TMetadata metadata, string name, Encoding encoding = null, int bufferSize = -1)
            where TMetadata : IFileValueMetadata => new FileValueWriter<TMetadata>(metadata, name, encoding, bufferSize);
        // public static FileValueWriter<TMetadata> Create<TMetadata>(TMetadata metadata, string name, Stream stream, Encoding encoding = null, int bufferSize = -1)
        //     where TMetadata : IFileValueMetadata => new FileValueWriter<TMetadata>(metadata, name, stream, encoding, bufferSize);
        public static FileValueWriter<NoSourceFileValueMetadata> Create(string name, Encoding encoding = null, int bufferSize = -1)
            => new FileValueWriter<NoSourceFileValueMetadata>(new NoSourceFileValueMetadata(""), name, encoding, bufferSize);
        // public static FileValueWriter<NoSourceFileValueMetadata> Create<TMetadata>(string name, Stream stream, Encoding encoding = null, int bufferSize = -1)
        //     => new FileValueWriter<NoSourceFileValueMetadata>(new NoSourceFileValueMetadata(""), name, stream, encoding, bufferSize);
    }
}