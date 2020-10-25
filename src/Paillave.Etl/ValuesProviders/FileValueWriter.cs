using System;
using System.IO;
using System.Text;
using System.Text.Json;
namespace Paillave.Etl.ValuesProviders
{
    public class FileValueWriter<TMetadata> : StreamWriter, IFileValue
        where TMetadata : IFileValueMetadata
    {
        public FileValueWriter(TMetadata metadata, string name, Encoding encoding = null, int bufferSize = -1)
            : this(metadata, name, new MemoryStream(), encoding, bufferSize) { }
        public FileValueWriter(TMetadata metadata, string name, Stream stream, Encoding encoding = null, int bufferSize = -1)
            : base(stream, encoding, bufferSize, true) => Name = name;
        public TMetadata Metadata { get; }
        public virtual string SourceType => this.Metadata.Type;
        public Type MetadataType => typeof(TMetadata);
        IFileValueMetadata IFileValue.Metadata => this.Metadata;
        public string Name { get; }
        public void Delete() { }
        public Stream GetContent()
        {
            this.Flush();
            MemoryStream ms = new MemoryStream();
            this.BaseStream.Seek(0, SeekOrigin.Begin);
            this.BaseStream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
    public static class FileValueWriter
    {
        public static FileValueWriter<TMetadata> Create<TMetadata>(TMetadata metadata, string name, Encoding encoding = null, int bufferSize = -1)
            where TMetadata : IFileValueMetadata => new FileValueWriter<TMetadata>(metadata, name, encoding, bufferSize);
        public static FileValueWriter<TMetadata> Create<TMetadata>(TMetadata metadata, string name, Stream stream, Encoding encoding = null, int bufferSize = -1)
            where TMetadata : IFileValueMetadata => new FileValueWriter<TMetadata>(metadata, name, stream, encoding, bufferSize);
        public static FileValueWriter<NoSourceFileValueMetadata> Create(string name, Encoding encoding = null, int bufferSize = -1)
            => new FileValueWriter<NoSourceFileValueMetadata>(new NoSourceFileValueMetadata(""), name, encoding, bufferSize);
        public static FileValueWriter<NoSourceFileValueMetadata> Create<TMetadata>(string name, Stream stream, Encoding encoding = null, int bufferSize = -1)
            => new FileValueWriter<NoSourceFileValueMetadata>(new NoSourceFileValueMetadata(""), name, stream, encoding, bufferSize);
    }
}