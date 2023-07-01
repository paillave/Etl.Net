using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Microsoft.Extensions.FileSystemGlobbing;
using Renci.SshNet;
using System.Linq;

namespace Paillave.Etl.Sftp
{
    public class SftpFileValueProvider : FileValueProviderBase<SftpAdapterConnectionParameters, SftpAdapterProviderParameters>
    {
        public SftpFileValueProvider(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters)
            : base(code, name, connectionName, connectionParameters, providerParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Provide(Action<IFileValue> pushFileValue, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters, CancellationToken cancellationToken, IExecutionContext context)
        {
            var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
            var matcher = new Matcher().AddInclude(searchPattern);
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");

            var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => GetFileList(connectionParameters, providerParameters));
            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (matcher.Match(file.Name).HasMatches)
                    pushFileValue(new SftpFileValue(connectionParameters, folder, file.Name, this.Code, this.Name, this.ConnectionName));
            }
        }
        private Renci.SshNet.Sftp.SftpFile[] GetFileList(SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters)
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
            var connectionInfo = connectionParameters.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                return client.ListDirectory(folder).ToArray();
            }
        }
        protected override void Test(SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters)
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
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