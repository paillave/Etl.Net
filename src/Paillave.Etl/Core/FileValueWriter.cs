using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
namespace Paillave.Etl.Core
{
    public class FileValueWriter<TMetadata> : IFileValue
        where TMetadata : IFileValueMetadata
    {
        private readonly StreamWriter _streamWriter;
        public FileValueWriter(TMetadata metadata, string name, Encoding encoding = null, int bufferSize = -1)
            => (_streamWriter, Name, Metadata) = (new StreamWriter(new MemoryStream(), encoding, bufferSize, true), name, metadata);

        public FileValueWriter(TMetadata metadata, Dictionary<string, IEnumerable<Destination>> destinations, string name, Encoding encoding = null, int bufferSize = -1)
            => (_streamWriter, Name, Metadata, Destinations) = (new StreamWriter(new MemoryStream(), encoding, bufferSize, true), name, metadata, destinations);

        public FileValueWriter<TMetadata> Write(string format, params object[] arg)
        {
            _streamWriter.Write(format, arg);
            return this;
        }
        public FileValueWriter<TMetadata> Write(string format, object arg0, object arg1, object arg2)
        {
            _streamWriter.Write(format, arg0, arg1, arg2);
            return this;
        }
        public FileValueWriter<TMetadata> Write(string format, object arg0)
        {
            _streamWriter.Write(format, arg0);
            return this;
        }
        public FileValueWriter<TMetadata> Write(ReadOnlySpan<char> buffer)
        {
            _streamWriter.Write(buffer);
            return this;
        }
        public FileValueWriter<TMetadata> Write(char[] buffer, int index, int count)
        {
            _streamWriter.Write(buffer, index, count);
            return this;
        }
        public FileValueWriter<TMetadata> Write(char[] buffer)
        {
            _streamWriter.Write(buffer);
            return this;
        }
        public FileValueWriter<TMetadata> Write(char value)
        {
            _streamWriter.Write(value);
            return this;
        }
        public FileValueWriter<TMetadata> Write(string value)
        {
            _streamWriter.Write(value);
            return this;
        }
        public FileValueWriter<TMetadata> WriteLine(string format, params object[] arg)
        {
            _streamWriter.WriteLine(format, arg);
            return this;
        }
        public FileValueWriter<TMetadata> WriteLine(string format, object arg0, object arg1, object arg2)
        {
            _streamWriter.WriteLine(format, arg0, arg1, arg2);
            return this;
        }
        public FileValueWriter<TMetadata> WriteLine(string format, object arg0, object arg1)
        {
            _streamWriter.WriteLine(format, arg0, arg1);
            return this;
        }
        public FileValueWriter<TMetadata> WriteLine(string value)
        {
            _streamWriter.WriteLine(value);
            return this;
        }
        public FileValueWriter<TMetadata> WriteLine(ReadOnlySpan<char> buffer)
        {
            _streamWriter.WriteLine(buffer);
            return this;
        }
        public FileValueWriter<TMetadata> WriteLine(string format, object arg0)
        {
            _streamWriter.WriteLine(format, arg0);
            return this;
        }


        public Dictionary<string, IEnumerable<Destination>> Destinations { get; }
        public TMetadata Metadata { get; }
        public virtual string SourceType => this.Metadata.Type;
        public Type MetadataType => typeof(TMetadata);
        IFileValueMetadata IFileValue.Metadata => new MiscFileValueMetadata { ExtraMetadata = this.Metadata, Destinations = this.Destinations };
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
        public static FileValueWriter<TMetadata> Create<TMetadata>(TMetadata metadata, Dictionary<string, IEnumerable<Destination>> destinations, string name, Encoding encoding = null, int bufferSize = -1)
            where TMetadata : IFileValueMetadata => new FileValueWriter<TMetadata>(metadata, destinations, name, encoding, bufferSize);
        public static FileValueWriter<NoSourceFileValueMetadata> Create(string name, Encoding encoding = null, int bufferSize = -1)
            => new FileValueWriter<NoSourceFileValueMetadata>(new NoSourceFileValueMetadata(""), name, encoding, bufferSize);
        public static FileValueWriter<NoSourceFileValueMetadata> Create(string name, Dictionary<string, IEnumerable<Destination>> destinations, Encoding encoding = null, int bufferSize = -1)
            => new FileValueWriter<NoSourceFileValueMetadata>(new NoSourceFileValueMetadata(""), destinations, name, encoding, bufferSize);
    }
    public class MiscFileValueMetadata : FileValueMetadataBase, IFileValueWithDestinationMetadata
    {
        public Dictionary<string, IEnumerable<Destination>> Destinations { get; set; }
        public object ExtraMetadata { get; set; }
    }
}