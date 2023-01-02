using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Dropbox
{
    public class DropboxFileValue : FileValueBase<DropboxFileValueMetadata>
    {
        public override string Name { get; }
        private readonly string _folder;
        private readonly IDropboxConnectionInfo _connectionInfo;

        public DropboxFileValue(IDropboxConnectionInfo connectionInfo, string folder, string fileName)
            : this(connectionInfo, folder, fileName, null, null, null) { }
        public DropboxFileValue(IDropboxConnectionInfo connectionInfo, string folder, string fileName, string connectorCode, string connectionName, string connectorName)
            : base(new DropboxFileValueMetadata
            {
                Folder = folder,
                Name = fileName,
                ConnectorCode = connectorCode,
                ConnectionName = connectionName,
                ConnectorName = connectorName
            }) => (Name, _folder, _connectionInfo) = (fileName, folder, connectionInfo);
        protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
        protected void DeleteFileSingleTime()
        {
            var path = $"/{Path.Combine(_folder ?? "", Name ?? "")}".Replace("\\","/").Replace("//","/");
            using (var client = _connectionInfo.CreateConnectionInfo())
                client.Files.DeleteV2Async(path).Wait();
        }
        public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
        private Stream GetContentSingleTime()
        {
            using (var client = _connectionInfo.CreateConnectionInfo())
            {
                var path = $"/{Path.Combine(_folder ?? "", Name ?? "")}".Replace("\\","/").Replace("//","/");
                var response = client.Files.DownloadAsync(path).Result;
                var dl = response.GetContentAsByteArrayAsync().Result;
                return new MemoryStream(dl);
            }
        }
    }
    public class DropboxFileValueMetadata : FileValueMetadataBase
    {
        public string Folder { get; set; }
        public string Name { get; set; }
    }
}
