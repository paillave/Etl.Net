using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Renci.SshNet;
using System.Text;

namespace Paillave.Etl.Sftp.ValuesProviders
{
    public class SftpFilesValue
    {
        public string Name { get; }
        private readonly string _path;
        private readonly SftpConnectionInfo _connectionInfo;

        public SftpFilesValue(SftpConnectionInfo connectionInfo, string path, string name)
        {
            this.Name = name;
            this._path = path;
            this._connectionInfo = connectionInfo;
        }

        public void DownloadToLocalFile(string filePath)
        {
            var connectionInfo = _connectionInfo.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                using (FileStream fs = File.OpenWrite(filePath))
                    client.OpenRead(Path.Combine(_path, Name)).CopyTo(fs);
            }
        }

        public Stream GetContent()
        {
            var connectionInfo = _connectionInfo.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                return new MemoryStream(client.ReadAllBytes(Path.Combine(_path, Name)));
            }
        }
    }

    public class SftpFilesValuesProviderArgs
    {
        public string Path { get; set; }
    }

    public class SftpFilesValuesProvider
    {
        public void PushValues(SftpFilesValuesProviderArgs args, SftpConnectionInfo connectionInfo, Action<SftpFilesValue> pushValue)
        {
            GetFiles(connectionInfo, args.Path).ToList().ForEach(pushValue);
        }

        private IEnumerable<SftpFilesValue> GetFiles(SftpConnectionInfo sftpConnectionInfo, string path)
        {
            var connectionInfo = sftpConnectionInfo.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                var files = client.ListDirectory(path);
                foreach (var file in files)
                    yield return new SftpFilesValue(sftpConnectionInfo, path, file.Name);
            }
        }
    }
}
