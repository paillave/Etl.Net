using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using System.Net;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Paillave.Etl.Ftp
{
    public class FtpAdapterConnectionParameters : IFtpConnectionInfo
    {
        public string RootFolder { get; set; }
        [Required]
        public string Server { get; set; }
        public int PortNumber { get; set; } = 21;
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }
    public class FtpAdapterProviderParameters
    {
        public string SubFolder { get; set; }
        public string FileNamePattern { get; set; }
    }
    public class FtpAdapterProcessorParameters
    {
        public string SubFolder { get; set; }
    }
    public class FtpProviderProcessorAdapter : ProviderProcessorAdapterBase<FtpAdapterConnectionParameters, FtpAdapterProviderParameters, FtpAdapterProcessorParameters>
    {
        public override string Description => "Get and save files on an FTP server";
        public override string Name => "Ftp";
        private class FtpFileValueProvider : FileValueProviderBase<FtpAdapterConnectionParameters, FtpAdapterProviderParameters>
        {
            public FtpFileValueProvider(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters)
                : base(code, name, connectionName, connectionParameters, providerParameters) { }
            public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
            public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
            protected override void Provide(Action<IFileValue> pushFileValue, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters, CancellationToken cancellationToken, IDependencyResolver resolver)
            {
                var folder = Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
                var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
                UriBuilder uriBuilder = new UriBuilder("ftp", connectionParameters.Server, connectionParameters.PortNumber, folder);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);
                var matcher = new Matcher().AddInclude(searchPattern);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    while (!reader.EndOfStream)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        var fileName = reader.ReadLine();
                        fileName = Path.GetFileName(fileName);
                        if (matcher.Match(fileName).HasMatches)
                            pushFileValue(new FtpFileValue(connectionParameters, folder, fileName, this.Code, this.Name, this.ConnectionName));
                    }
            }

            protected override void Test(FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters)
            {
                var folder = Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
                var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
                UriBuilder uriBuilder = new UriBuilder("ftp", connectionParameters.Server, connectionParameters.PortNumber, folder);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);
                var matcher = new Matcher().AddInclude(searchPattern);
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    while (!reader.EndOfStream)
                        reader.ReadLine();
            }
        }
        private class FtpFileValueProcessor : FileValueProcessorBase<FtpAdapterConnectionParameters, FtpAdapterProcessorParameters>
        {
            public FtpFileValueProcessor(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters processorParameters)
                : base(code, name, connectionName, connectionParameters, processorParameters) { }
            public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
            public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
            protected override void Process(IFileValue fileValue, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver)
            {
                var folder = Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
                UriBuilder uriBuilder = new UriBuilder("ftp", connectionParameters.Server, connectionParameters.PortNumber, Path.Combine(folder, fileValue.Name));

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);

                var stream = fileValue.GetContent();
                byte[] fileContents;
                stream.Position = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    fileContents = ms.ToArray();
                }

                request.ContentLength = fileContents.Length;

                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(fileContents, 0, fileContents.Length);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    if (response.StatusCode != FtpStatusCode.CommandOK)
                        throw new FtpUploadException(response.StatusCode, response.StatusDescription);
                push(fileValue);
            }

            protected override void Test(FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters processorParameters)
            {
                var fileName = Guid.NewGuid().ToString();
                var folder = Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
                UriBuilder uriBuilder = new UriBuilder("ftp", connectionParameters.Server, connectionParameters.PortNumber, Path.Combine(folder, fileName));

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                request.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);

                var stream = new MemoryStream();
                byte[] fileContents;
                stream.Position = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    fileContents = ms.ToArray();
                }

                request.ContentLength = fileContents.Length;

                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(fileContents, 0, fileContents.Length);

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    if (response.StatusCode != FtpStatusCode.CommandOK)
                        throw new FtpUploadException(response.StatusCode, response.StatusDescription);


                FtpWebRequest deleteRequest = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
                deleteRequest.Method = WebRequestMethods.Ftp.DeleteFile;

                deleteRequest.Credentials = new NetworkCredential(connectionParameters.Login, connectionParameters.Password);
                using (FtpWebResponse response = (FtpWebResponse)deleteRequest.GetResponse())
                    if (!new[] { FtpStatusCode.CommandOK, FtpStatusCode.FileActionOK }.Contains(response.StatusCode))
                        throw new Exception("Ftp delete request failed");
            }
        }
        protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters inputParameters)
            => new FtpFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);
        protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters outputParameters)
            => new FtpFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
    }
}