using System.IO;
using FluentFtp;
using Paillave.Etl.Core;


namespace Paillave.Etl.HttpExtension;
{
    public class HttpFileValue : FileValueBase<HttpFileValueMetadata>
    {
        public override string Name { get; }
        private readonly string _folder;
        private readonly IHttpConnectionInfo _connectionInfo;

        public HttpFileValue(IHttpConnectionInfo connectionInfo, string folder, string fileName)
            : this(connectionInfo, folder, fileName, null, null, null) { }

        public HttpFileValue(
            IHttpConnectionInfo connectionInfo,
            string folder,
            string fileName,
            string connectorCode,
            string connectorName,
            string connectionName
        )
            : base(
                new HttpFileValueMetadata
                {
                    Server = connectionInfo.Server,
                    Folder = folder,
                    Name = fileName,
                    ConnectorCode = connectorCode,
                    ConnectionName = connectionName,
                    ConnectorName = connectorName,
                }
            ) => (Name, _folder, _connectionInfo) = (fileName, folder, connectionInfo);

        protected override void DeleteFile() =>
            ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);

        protected void DeleteFileSingleTime()
        {
            var pathToDelete = StringEx.ConcatenatePath(_folder, this.Name).Replace('\\', '/');
            using (HttpClient client = _connectionInfo.CreateHttpClient())
                client.DeleteFile(pathToDelete);
        }

        public override Stream GetContent() =>
            ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);

        private Stream GetContentSingleTime()
        {
            using (HttpClient client = _connectionInfo.CreateHttpClient())
            {
                MemoryStream ms = new MemoryStream();
                var pathToDownload = StringEx
                    .ConcatenatePath(_folder, this.Name)
                    .Replace('\\', '/');
                if (!client.DownloadStream(ms, pathToDownload))
                    throw new System.Exception($"File {pathToDownload} failed to be downloaded");
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }

        public override StreamWithResource OpenContent()
        {
            HttpClient client = _connectionInfo.CreateHttpClient();
            var pathToDownload = StringEx.ConcatenatePath(_folder, this.Name).Replace('\\', '/');
            var targetPath = Path.GetTempFileName();
            if (client.DownloadFile(targetPath, pathToDownload) != HttpStatus.Success)
                throw new System.Exception($"File {pathToDownload} failed to be downloaded");
            return new StreamWithResource(new DeletetableFileStream(targetPath), client);
        }

        private class DeletetableFileStream : Stream
        {
            private readonly FileStream _fileStream;

            public DeletetableFileStream(string path)
                : this(File.OpenRead(path)) { }

            public DeletetableFileStream(FileStream fileStream)
                : base() => (_fileStream) = (fileStream);

            public override bool CanRead => _fileStream.CanRead;

            public override bool CanSeek => _fileStream.CanSeek;

            public override bool CanWrite => _fileStream.CanWrite;

            public override long Length => _fileStream.Length;

            public override long Position
            {
                get => _fileStream.Position;
                set => _fileStream.Position = value;
            }

            public override void Flush()
            {
                _fileStream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _fileStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _fileStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _fileStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _fileStream.Write(buffer, offset, count);
            }

            protected override void Dispose(bool disposing)
            {
                _fileStream.Dispose();
                try
                {
                    File.Delete(_fileStream.Name);
                }
                catch { }
            }
        }
    }

    public class HttpFileValueMetadata : FileValueMetadataBase
    {
        public string Server { get; set; }
        public string Folder { get; set; }
        public string Name { get; set; }
    }
}
