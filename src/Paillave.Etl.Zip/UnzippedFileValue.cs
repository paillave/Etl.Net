using System.Collections.Generic;
using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Zip
{
    public class UnzippedFileValue<TMetadata> : FileValueBase<TMetadata> where TMetadata : IFileValueMetadata
    {
        private readonly Stream _stream;
        private readonly IFileValue _underlyingFileValue;
        private readonly HashSet<string> _pathsNotFlaggedForDelete;
        private readonly string _innerPath;
        public override string Name { get; }
        public UnzippedFileValue(Stream stream, string name, TMetadata metadata, IFileValue underlyingFileValue, HashSet<string> pathsNotFlaggedForDelete, string innerPath)
            : base(metadata) => (_stream, Name, _underlyingFileValue, _pathsNotFlaggedForDelete, _innerPath) = (stream, name, underlyingFileValue, pathsNotFlaggedForDelete, innerPath);
        public override Stream GetContent()
        {
            var ms = new MemoryStream();
            _stream.Seek(0, SeekOrigin.Begin);
            _stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
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

        public override Stream OpenContent()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            return _stream;
        }
    }
}