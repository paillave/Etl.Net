using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using System.Net;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Paillave.Etl.Ftp
{
    public class FtpFileValueProvider : FileValueProviderBase<FtpAdapterConnectionParameters, FtpAdapterProviderParameters>
    {
        public FtpFileValueProvider(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters)
            : base(code, name, connectionName, connectionParameters, providerParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Provide(Action<IFileValue> pushFileValue, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
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
}