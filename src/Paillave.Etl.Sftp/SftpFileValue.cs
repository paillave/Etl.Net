using System.IO;
using Renci.SshNet;
using Paillave.Etl.Core;

namespace Paillave.Etl.Sftp
{
    public class SftpFileValue : FileValueBase<SftpFileValueMetadata>
    {
        public override string Name { get; }
        private readonly string _folder;
        private readonly ISftpConnectionInfo _connectionInfo;

        public SftpFileValue(ISftpConnectionInfo connectionInfo, string folder, string fileName)
            : this(connectionInfo, folder, fileName, null, null, null) { }
        public SftpFileValue(ISftpConnectionInfo connectionInfo, string folder, string fileName, string connectorCode, string connectionName, string connectorName)
            : base(new SftpFileValueMetadata
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
            var connectionInfo = _connectionInfo.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                client.DeleteFile(Path.Combine(_folder, Name));
            }
        }
        public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
        private Stream GetContentSingleTime()
        {
            var connectionInfo = _connectionInfo.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                return new MemoryStream(client.ReadAllBytes(Path.Combine(_folder, Name)));
            }
        }
        private Stream OpenContentSingleTime()
        {
            var connectionInfo = _connectionInfo.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                return client.OpenRead(Path.Combine(_folder, Name));
            }
        }

        public override Stream OpenContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, OpenContentSingleTime);
    }
    public class SftpFileValueMetadata : FileValueMetadataBase
    {
        public string Server { get; set; }
        public string Folder { get; set; }
        public string Name { get; set; }
    }
}
