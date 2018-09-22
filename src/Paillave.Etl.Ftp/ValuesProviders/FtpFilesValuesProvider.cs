using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Paillave.Etl.Ftp.ValuesProviders
{
    public class FtpFilesValue
    {
        public string Name { get; }
        private readonly string _path;
        private readonly FtpConnectionInfo _connectionInfo;

        public FtpFilesValue(FtpConnectionInfo connectionInfo, string path, string name)
        {
            this.Name = name;
            this._path = path;
            this._connectionInfo = connectionInfo;
        }

        public void DownloadToLocalFile(string filePath)
        {
            UriBuilder uriBuilder = new UriBuilder("ftp", _connectionInfo.Server, _connectionInfo.PortNumber, Path.Combine(_path, this.Name).Replace('\\', '/'));
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            request.Credentials = new NetworkCredential(_connectionInfo.Login, _connectionInfo.Password);
            MemoryStream ms = new MemoryStream();
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (FileStream fs = File.OpenWrite(filePath))
                response.GetResponseStream().CopyTo(fs);
        }

        public Stream GetContent()
        {
            UriBuilder uriBuilder = new UriBuilder("ftp", _connectionInfo.Server, _connectionInfo.PortNumber, Path.Combine(_path, this.Name).Replace('\\', '/'));
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            request.Credentials = new NetworkCredential(_connectionInfo.Login, _connectionInfo.Password);
            MemoryStream ms = new MemoryStream();
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                response.GetResponseStream().CopyTo(ms);
            return ms;
        }
    }

    public class FtpFilesValuesProviderArgs
    {
        public string Path { get; set; }
    }

    public class FtpFilesValuesProvider : ValuesProviderBase<FtpFilesValuesProviderArgs, FtpConnectionInfo, FtpFilesValue>
    {
        public FtpFilesValuesProvider() : base(false)
        {
        }

        public void Dispose() { }
        protected override void PushValues(FtpConnectionInfo connectionInfo, FtpFilesValuesProviderArgs args, Action<FtpFilesValue> pushValue)
        {
            GetFiles(connectionInfo, args.Path).ToList().ForEach(pushValue);
        }
        //protected override void PushValues(FtpFilesValuesProviderArgs args, Action<FtpConnectionInfo, FtpFilesValue> pushValue)
        //{
        //}
        private IEnumerable<FtpFilesValue> GetFiles(FtpConnectionInfo connectionInfo, string path)
        {
            UriBuilder uriBuilder = new UriBuilder("ftp", connectionInfo.Server, connectionInfo.PortNumber, path);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            request.Credentials = new NetworkCredential(connectionInfo.Login, connectionInfo.Password);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                while (!reader.EndOfStream)
                    yield return new FtpFilesValue(connectionInfo, path, reader.ReadLine());
        }
    }
}
