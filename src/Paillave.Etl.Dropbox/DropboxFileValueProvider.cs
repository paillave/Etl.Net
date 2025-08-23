using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Linq;
using Dropbox.Api.Files;
using System.Text.Json;

namespace Paillave.Etl.Dropbox
{
    public class DropboxFileValueProvider(string code, string name, string connectionName, DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProviderParameters providerParameters) : FileValueProviderBase<DropboxAdapterConnectionParameters, DropboxAdapterProviderParameters>(code, name, connectionName, connectionParameters, providerParameters)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Provide(object input, Action<IFileValue, FileReference> pushFileValue, DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProviderParameters providerParameters, CancellationToken cancellationToken)
        {
            var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
            var matcher = new Matcher().AddInclude(searchPattern);
            var folder = $"/{Path.Combine(connectionParameters.RootFolder ?? "", providerParameters.SubFolder ?? "")}".Replace("\\", "/").Replace("//", "/");

            var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => DropboxFileValueProvider.GetFileList(connectionParameters, providerParameters));
            foreach (var file in files.Entries.Where(i => i.IsFile && !i.IsDeleted))
            {
                if (cancellationToken.IsCancellationRequested) break;
                if (matcher.Match(file.Name).HasMatches)
                {
                    var fileValue = new DropboxFileValue(connectionParameters, folder, file.Name, this.Code, this.Name, this.ConnectionName);
                    var fileReference = new FileReference(fileValue.Name, this.Code, JsonSerializer.Serialize(new FileSpecificData { FileName = file.Name, Folder = folder }));
                    pushFileValue(fileValue, fileReference);
                }
            }
        }
        private class FileSpecificData
        {
            public required string Folder { get; set; }
            public required string FileName { get; set; }
        }

        private static ListFolderResult GetFileList(DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProviderParameters providerParameters)
        {
            var folder = $"/{Path.Combine(connectionParameters.RootFolder ?? "", providerParameters.SubFolder ?? "")}".Replace("\\", "/");
            using var client = connectionParameters.CreateConnectionInfo();
            return client.Files.ListFolderAsync(folder == "/" ? "" : folder.Replace("\\", "/")).Result;
        }
        protected override void Test(DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProviderParameters providerParameters)
        {
            var folder = $"/{Path.Combine(connectionParameters.RootFolder ?? "", providerParameters.SubFolder ?? "")}".Replace("\\", "/").Replace("//", "/");
            var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
            var matcher = new Matcher().AddInclude(searchPattern);
            using var client = connectionParameters.CreateConnectionInfo();
            client.Files.ListFolderAsync(folder == "/" ? "" : folder);
        }

        public override IFileValue Provide(string name, string fileSpecific)
        {
            var fileSpecificData = JsonSerializer.Deserialize<FileSpecificData>(fileSpecific) ?? throw new Exception("Invalid file specific");
            return new DropboxFileValue(connectionParameters, fileSpecificData.Folder, fileSpecificData.FileName);
        }
    }
}