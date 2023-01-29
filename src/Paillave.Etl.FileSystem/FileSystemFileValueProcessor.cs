using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;

namespace Paillave.Etl.FileSystem
{
    public class FileSystemFileValueProcessor : FileValueProcessorBase<FileSystemAdapterConnectionParameters, FileSystemAdapterProcessorParameters>
    {
        public FileSystemFileValueProcessor(string code, string name, string outputFolder)
        : base(code, name, name, new FileSystemAdapterConnectionParameters { RootFolder = outputFolder }, new FileSystemAdapterProcessorParameters()) { }
        public FileSystemFileValueProcessor(string code, string name, string connectionName, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProcessorParameters processorParameters)
        : base(code, name, connectionName, connectionParameters, processorParameters) { }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override void Process(IFileValue fileValue, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            using var l = fileValue.Get(processorParameters.UseStreamCopy);
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (processorParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
            var outputFilePath = Path.Combine(folder, fileValue.Name);
            using (var fileStream = File.OpenWrite(outputFilePath))
                l.CopyTo(fileStream);
            push(fileValue);
        }

        protected override void Test(FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProcessorParameters processorParameters)
        {
            var fileName = Guid.NewGuid().ToString();
            var l = new MemoryStream();
            l.Seek(0, SeekOrigin.Begin);
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (processorParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
            using (var fileStream = File.OpenWrite(Path.Combine(folder, fileName)))
                l.CopyTo(fileStream);
            File.Delete(Path.Combine(folder, fileName));
        }
    }
}