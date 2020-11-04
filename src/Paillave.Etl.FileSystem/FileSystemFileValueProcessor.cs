using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;

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
        protected override void Process(IFileValue fileValue, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var l = fileValue.GetContent();
            l.Seek(0, SeekOrigin.Begin);
            var outputFilePath=Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "", fileValue.Name);
            using (var fileStream = File.OpenWrite(outputFilePath))
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
}