using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using Microsoft.Extensions.FileSystemGlobbing;
// using Renci.SshNet;
using System.Linq;
using Dropbox.Api.Files;

namespace Paillave.Etl.Dropbox
{
    public class DropboxFileValueProvider : FileValueProviderBase<DropboxAdapterConnectionParameters, DropboxAdapterProviderParameters>
    {
        public DropboxFileValueProvider(string code, string name, string connectionName, DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProviderParameters providerParameters)
            : base(code, name, connectionName, connectionParameters, providerParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Provide(Action<IFileValue> pushFileValue, DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProviderParameters providerParameters, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
            var matcher = new Matcher().AddInclude(searchPattern);
            var folder = $"/{Path.Combine(connectionParameters.RootFolder ?? "", providerParameters.SubFolder ?? "")}".Replace("\\","/").Replace("//","/");

            var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => GetFileList(connectionParameters, providerParameters));
            foreach (var file in files.Entries.Where(i => i.IsFile && !i.IsDeleted))
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (matcher.Match(file.Name).HasMatches)
                    pushFileValue(new DropboxFileValue(connectionParameters, folder, file.Name, this.Code, this.Name, this.ConnectionName));
            }
        }
        private ListFolderResult GetFileList(DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProviderParameters providerParameters)
        {
            var folder = $"/{Path.Combine(connectionParameters.RootFolder ?? "", providerParameters.SubFolder ?? "")}".Replace("\\","/");
            using (var client = connectionParameters.CreateConnectionInfo())
                return client.Files.ListFolderAsync(folder == "/" ? "" : folder.Replace("\\","/")).Result;
        }
        protected override void Test(DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProviderParameters providerParameters)
        {
            var folder = $"/{Path.Combine(connectionParameters.RootFolder ?? "", providerParameters.SubFolder ?? "")}".Replace("\\","/").Replace("//","/");
            var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
            var matcher = new Matcher().AddInclude(searchPattern);
            using (var client = connectionParameters.CreateConnectionInfo())
                client.Files.ListFolderAsync(folder == "/" ? "" : folder);
        }
    }
}