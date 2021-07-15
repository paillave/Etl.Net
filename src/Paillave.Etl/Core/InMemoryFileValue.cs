using System.IO;
using System.Text;
using System.Text.Json;
namespace Paillave.Etl.Core
{
    public class InMemoryFileValue<TMetadata> : FileValueBase<TMetadata> where TMetadata : IFileValueMetadata
    {
        private readonly Stream _stream;
        public InMemoryFileValue(Stream stream, string name, TMetadata metadata)
            : base(metadata) => (_stream, Name) = (stream, name);
        public override string Name { get; }
        public override Stream GetContent() => _stream;
        protected override void DeleteFile() { }
    }
    public static class FileValue
    {
        public static IFileValue Create<TMetadata>(Stream stream, string name, TMetadata metadata)
            where TMetadata : IFileValueMetadata
            => new InMemoryFileValue<TMetadata>(stream, name, metadata);
        public static IFileValue Create(Stream stream, string name, string type)
            => new InMemoryFileValue<NoSourceFileValueMetadata>(stream, name, new NoSourceFileValueMetadata(type));
    }
}