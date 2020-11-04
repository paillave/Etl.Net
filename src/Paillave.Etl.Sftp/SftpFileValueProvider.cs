using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using Microsoft.Extensions.FileSystemGlobbing;
using Renci.SshNet;

namespace Paillave.Etl.Sftp
{
    public class SftpFileValueProvider : FileValueProviderBase<SftpAdapterConnectionParameters, SftpAdapterProviderParameters>
    {
        public SftpFileValueProvider(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters)
            : base(code, name, connectionName, connectionParameters, providerParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Provide(Action<IFileValue> pushFileValue, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var folder = Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
            var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
            var matcher = new Matcher().AddInclude(searchPattern);
            var connectionInfo = connectionParameters.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                var files = client.ListDirectory(folder);
                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    if (matcher.Match(file.Name).HasMatches)
                        pushFileValue(new SftpFileValue(connectionParameters, folder, file.Name, this.Code, this.Name, this.ConnectionName));
                }
            }
        }

        protected override void Test(SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters)
        {
            var folder = Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
            var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
            var matcher = new Matcher().AddInclude(searchPattern);
            var connectionInfo = connectionParameters.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                client.ListDirectory(folder);
            }
        }
    }
}