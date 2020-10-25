using Paillave.Etl.ValuesProviders;
using System;
using System.IO;
using System.Linq;
using System.Net;

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
        protected override void DeleteFile()
        {
            UriBuilder uriBuilder = new UriBuilder("ftp", _connectionInfo.Server, _connectionInfo.PortNumber, Path.Combine(_folder, this.Name).Replace('\\', '/'));
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.DeleteFile;

            request.Credentials = new NetworkCredential(_connectionInfo.Login, _connectionInfo.Password);
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                if (!new[] { FtpStatusCode.CommandOK, FtpStatusCode.FileActionOK }.Contains(response.StatusCode))
                    throw new Exception("Ftp request failed");
        }
        public override Stream GetContent()
        {
            UriBuilder uriBuilder = new UriBuilder("ftp", _connectionInfo.Server, _connectionInfo.PortNumber, Path.Combine(_folder, this.Name).Replace('\\', '/'));
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            request.Credentials = new NetworkCredential(_connectionInfo.Login, _connectionInfo.Password);
            MemoryStream ms = new MemoryStream();
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                response.GetResponseStream().CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
    public class FtpFileValueMetadata : FileValueMetadataBase
    {
        public string Server { get; set; }
        public string Folder { get; set; }
        public string Name { get; set; }
        public string ConnectorCode { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectorName { get; set; }
    }
}
