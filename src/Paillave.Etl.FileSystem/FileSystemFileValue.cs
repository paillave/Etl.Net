using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.ValuesProviders;
using System.IO;
using System.Text.Json;

namespace Paillave.Etl.FileSystem
{
    public class FileSystemFileValue : FileValueBase<FileSystemFileValueMetadata>
    {
        private FileInfo _fileInfo;
        private CollectionDisposableManager _disposableManager = new CollectionDisposableManager();
        public FileSystemFileValue(FileInfo fileInfo)
            : this(fileInfo, null, null, null) { }
        public FileSystemFileValue(FileInfo fileInfo, string connectorCode, string connectorName, string connectionName)
            : base(new FileSystemFileValueMetadata
            {
                Folder = fileInfo.Directory.FullName,
                ConnectorCode = connectorCode,
                ConnectionName = connectionName,
                ConnectorName = connectorName
            }) => _fileInfo = fileInfo;
        public override string Name => _fileInfo.Name;
        protected override void DeleteFile()
        {
            _disposableManager.Dispose();
            _fileInfo.Delete();
        }
        public override Stream GetContent() => _disposableManager.Set(_fileInfo.OpenRead());
    }
    public class FileSystemFileValueMetadata : FileValueMetadataBase
    {
        public string Folder { get; set; }
        public string ConnectorCode { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectorName { get; set; }
    }
}
