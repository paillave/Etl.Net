using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.FileSystem
{
    public class FileSystemAdapterConnectionParameters
    {
        [Required]
        public string RootFolder { get; set; }
    }
    public class FileSystemAdapterProviderParameters
    {
        public string SubFolder { get; set; }
        public string FileNamePattern { get; set; }
        public bool Recursive { get; set; }
    }
    public class FileSystemAdapterProcessorParameters
    {
        public string SubFolder { get; set; }
    }
    public class FileSystemProviderProcessorAdapter : ProviderProcessorAdapterBase<FileSystemAdapterConnectionParameters, FileSystemAdapterProviderParameters, FileSystemAdapterProcessorParameters>
    {
        public override string Description => "Get and save files on the local file system";
        public override string Name => "FileSystem";
        private class FileSystemFileValueProvider : FileValueProviderBase<FileSystemAdapterConnectionParameters, FileSystemAdapterProviderParameters>
        {
            public FileSystemFileValueProvider(string code, string name, string connectionName, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProviderParameters providerParameters)
                : base(code, name, connectionName, connectionParameters, providerParameters) { }
            public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
            public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
            protected override void Provide(Action<IFileValue> pushFileValue, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProviderParameters providerParameters, CancellationToken cancellationToken, IDependencyResolver resolver)
            {
                var folder = Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
                var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
                var files = Directory
                    .GetFiles(folder, searchPattern, providerParameters.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                    .ToList();
                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    pushFileValue(new FileSystemFileValue(new FileInfo(file), Code, Name, ConnectionName));
                }
            }
            protected override void Test(FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProviderParameters providerParameters)
            {
                var folder = Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
                var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
                var files = Directory
                    .GetFiles(folder, searchPattern, providerParameters.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
        }
        private class FileSystemFileValueProcessor : FileValueProcessorBase<FileSystemAdapterConnectionParameters, FileSystemAdapterProcessorParameters>
        {
            public FileSystemFileValueProcessor(string code, string name, string connectionName, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProcessorParameters processorParameters)
            : base(code, name, connectionName, connectionParameters, processorParameters) { }

            public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
            public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
            protected override void Process(IFileValue fileValue, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver)
            {
                var l = fileValue.GetContent();
                l.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.OpenWrite(Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "", fileValue.Name)))
                    l.CopyTo(fileStream);
                push(fileValue);
            }

            protected override void Test(FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProcessorParameters processorParameters)
            {
                var fileName = Guid.NewGuid().ToString();
                var l = new MemoryStream();
                l.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.OpenWrite(Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "", fileName)))
                    l.CopyTo(fileStream);
                File.Delete(Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "", fileName));
            }
        }
        protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProviderParameters inputParameters)
            => new FileSystemFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);

        protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProcessorParameters outputParameters)
            => new FileSystemFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
    }
}