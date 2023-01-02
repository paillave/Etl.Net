using FluentFTP;
using Paillave.Etl.Core;
using System.IO;

namespace Paillave.Etl.Ftp
{
    public class FtpFileValue : FileValueBase<FtpFileValueMetadata>
    {
        public override string Name { get; }
        private readonly string _folder;
        private readonly IFtpConnectionInfo _connectionInfo;
        public FtpFileValue(IFtpConnectionInfo connectionInfo, string folder, string fileName)
            : this(connectionInfo, folder, fileName, null, null, null) { }
        public FtpFileValue(IFtpConnectionInfo connectionInfo, string folder, string fileName, string connectorCode, string connectorName, string connectionName)
            : base(new FtpFileValueMetadata
            {
                Server = connectionInfo.Server,
                Folder = folder,
                Name = fileName,
                ConnectorCode = connectorCode,
                ConnectionName = connectionName,
                ConnectorName = connectorName
            }) => (Name, _folder, _connectionInfo) = (fileName, folder, connectionInfo);
        protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
        protected void DeleteFileSingleTime()
        {
            var pathToDelete = Path.Combine(_folder, this.Name).Replace('\\', '/');
            using (FtpClient client = _connectionInfo.CreateFtpClient())
                client.DeleteFile(pathToDelete);
        }
        public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
        private Stream GetContentSingleTime()
        {
            using (FtpClient client = _connectionInfo.CreateFtpClient())
            {
                MemoryStream ms = new MemoryStream();
                var pathToDownload = Path.Combine(_folder, this.Name).Replace('\\', '/');
                if (!client.DownloadStream(ms, pathToDownload))
                    throw new System.Exception($"File {pathToDownload} failed to be downloaded");
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }
    }
    public class FtpFileValueMetadata : FileValueMetadataBase
    {
        public string Server { get; set; }
        public string Folder { get; set; }
        public string Name { get; set; }
    }
}
