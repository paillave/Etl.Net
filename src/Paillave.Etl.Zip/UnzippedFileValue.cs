using System.Collections.Generic;
using System.IO;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Zip
{
    public class UnzippedFileValue<TMetadata> : FileValueBase<TMetadata> where TMetadata : IFileValueMetadata
    {
        private readonly Stream _stream;
        private readonly IFileValue _underlyingFileValue;
        private readonly HashSet<string> _pathsNotFlaggedForDelete;
        private readonly string _innerPath;
        public UnzippedFileValue(Stream stream, string name, TMetadata metadata, IFileValue underlyingFileValue, HashSet<string> pathsNotFlaggedForDelete, string innerPath)
            : base(metadata) => (_stream, Name, _underlyingFileValue, _pathsNotFlaggedForDelete, _innerPath) = (stream, name, underlyingFileValue, pathsNotFlaggedForDelete, innerPath);
        public override string Name { get; }
        public override Stream GetContent() => _stream;
        protected override void DeleteFile()
        {
            if (_pathsNotFlaggedForDelete.Contains(_innerPath))
            {
                _pathsNotFlaggedForDelete.Remove(_innerPath);
            }
            if (_pathsNotFlaggedForDelete.Count == 0)
            {
                _underlyingFileValue.Delete();
            }
        }
    }
}