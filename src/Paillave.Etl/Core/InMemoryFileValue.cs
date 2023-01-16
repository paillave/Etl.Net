using System.IO;
namespace Paillave.Etl.Core
{
    public class InMemoryFileValue<TMetadata> : FileValueBase<TMetadata> where TMetadata : IFileValueMetadata
    {
        private readonly Stream _stream;
        public InMemoryFileValue(Stream stream, string name, TMetadata metadata)
            : base(metadata) => (_stream, Name) = (stream, name);
        public override string Name { get; }
        public override Stream GetContent()
        {
            var ms = new MemoryStream();
            _stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
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