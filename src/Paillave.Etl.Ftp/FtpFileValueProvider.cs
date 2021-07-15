using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Microsoft.Extensions.FileSystemGlobbing;
using FluentFTP;
using System.Linq;

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
            var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
            var matcher = new Matcher().AddInclude(searchPattern);
            var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => GetFileList(connectionParameters, providerParameters));
            foreach (var item in files)
            {
                if (cancellationToken.IsCancellationRequested) break;
                var fullPath = item.FullName;
                var fileName = Path.GetFileName(fullPath);
                fileName = Path.GetFileName(fileName);
                if (matcher.Match(fileName).HasMatches)
                    pushFileValue(new FtpFileValue(connectionParameters, Path.GetDirectoryName(fullPath), fileName, this.Code, this.Name, this.ConnectionName));
            }
        }
        private FtpListItem[] GetFileList(FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters)
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
            using (FtpClient client = connectionParameters.CreateFtpClient())
            {
                client.Connect();
                return (providerParameters.Recursive ? client.GetListing(folder, FtpListOption.Recursive) : client.GetListing(folder)).Where(i => i.Type == FtpFileSystemObjectType.File).ToArray();
            }
        }
        protected override void Test(FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters)
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
            using (FtpClient client = connectionParameters.CreateFtpClient())
            {
                client.Connect();
                if (providerParameters.Recursive)
                    client.GetListing(folder, FtpListOption.Recursive);
                else
                    client.GetListing(folder);
            }
        }
    }
}