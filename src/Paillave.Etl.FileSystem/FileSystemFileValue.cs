using Paillave.Etl.Core;
using System.IO;

namespace Paillave.Etl.FileSystem
{
    public class FileSystemFileValue : FileValueBase<FileSystemFileValueMetadata>
    {
        private FileInfo _fileInfo;
        public FileSystemFileValue(FileInfo fileInfo)
            : this(fileInfo, null, null, null) { }
        public FileSystemFileValue(FileInfo fileInfo, string connectorCode, string connectorName, string connectionName)
            : base(new FileSystemFileValueMetadata
            {
                Folder = fileInfo.Directory?.FullName,
                ConnectorCode = connectorCode,
                ConnectionName = connectionName,
                ConnectorName = connectorName
            }) => _fileInfo = fileInfo;
        public override string Name => _fileInfo.Name;
        protected override void DeleteFile()
        {
            _fileInfo.Delete();
        }
        public override Stream GetContent()
        {
            var stream = new MemoryStream();
            using (var fileStream = _fileInfo.OpenRead())
                fileStream.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        public override StreamWithResource OpenContent() => new StreamWithResource(_fileInfo.OpenRead());
    }
    public class FileSystemFileValueMetadata : FileValueMetadataBase
    {
        public string? Folder { get; set; }
    }
}
