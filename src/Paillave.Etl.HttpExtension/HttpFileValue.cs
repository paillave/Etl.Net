using System.IO;
using FluentFtp;
using Paillave.Etl.Core;


namespace Paillave.Etl.HttpExtension;
{
    public class HttpFileValue : FileValueBase<HttpFileValueMetadata>
    {
        public override string Name { get; }
        private readonly IHttpConnectionInfo _connectionInfo;

        public HttpFileValue(IHttpConnectionInfo connectionInfo, string fileName)
            : this(connectionInfo, folder, fileName, null, null, null) { }

        public HttpFileValue(
            IHttpConnectionInfo connectionInfo,
            string fileName,
            string connectorCode,
            string connectorName,
            string connectionName
        )
            : base(new HttpFileValueMetadata{
                Url = connectionInfo.Url,
                HeaderParts = connectionInfo.HeaderParts,
                ConnectorCode = connectorCode,
                ConnectionName = connectionName,
                ConnectorName = connectorName,
            }) 
                => (Name, _connectionInfo) = (fileName, connectionInfo);

          
        public override Stream GetContent() => new MemoryStream(_content ?? new byte[0]);

        public override StreamWithResource OpenContent() => new(GetContent());

        protected override void DeleteFile() { }


        // protected override void DeleteFile() =>
        //     ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);

        // protected void DeleteFileSingleTime()
        // {
        //     // TODO: is there even the equivalent of a delete in the context of RESTful APIs?

        //     // var pathToDelete = StringEx.ConcatenatePath(_folder, this.Name).Replace('\\', '/');
        //     // using (HttpClient client = _connectionInfo.CreateHttpClient())
        //     //     client.DeleteFile(pathToDelete);
        // }

        // public override Stream GetContent() =>
        //     ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);

        // private Stream GetContentSingleTime()
        // {
        //     using (HttpClient client = _connectionInfo.CreateHttpClient())
        //     {
        //         // TODO : use _connectionInfo on client to get stuff

        //         // MemoryStream ms = new MemoryStream();
        //         // var pathToDownload = StringEx
        //         //     .ConcatenatePath(_folder, this.Name)
        //         //     .Replace('\\', '/');
        //         // if (!client.DownloadStream(ms, pathToDownload))
        //         //     throw new System.Exception($"File {pathToDownload} failed to be downloaded");
        //         // ms.Seek(0, SeekOrigin.Begin);
        //         // return ms;
        //     }
        // }
    }

    
    public class HttpFileValueMetadata : FileValueMetadataBase
    {
        public string Url { get; set; }
        public List<string> HeaderParts { get; set; }
        public string ConnexionType { get; set; }
    }
}
