using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using Dropbox.Api.Files;
// using Renci.SshNet;

namespace Paillave.Etl.Dropbox
{
    public class DropboxFileValueProcessor : FileValueProcessorBase<DropboxAdapterConnectionParameters, DropboxAdapterProcessorParameters>
    {
        public DropboxFileValueProcessor(string code, string name, string connectionName, DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProcessorParameters processorParameters)
            : base(code, name, connectionName, connectionParameters, processorParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Process(IFileValue fileValue, DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var path = $"/{Path.Combine(connectionParameters.RootFolder ?? "", processorParameters.SubFolder ?? "", fileValue.Name)}".Replace("\\","/").Replace("//","/");
            var stream = fileValue.GetContent();
            byte[] fileContents;
            stream.Position = 0;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                fileContents = ms.ToArray();
            }
            ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => UploadSingleTime(connectionParameters, fileContents, path));
            push(fileValue);
        }
        private void UploadSingleTime(DropboxAdapterConnectionParameters connectionParameters, byte[] fileContents, string filePath)
        {
            using (var client = connectionParameters.CreateConnectionInfo())
                client.Files.UploadAsync(new CommitInfo(filePath), new MemoryStream(fileContents)).Wait();
        }
        protected override void Test(DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProcessorParameters processorParameters)
        {
            var fileName = Guid.NewGuid().ToString();
            var path = $"/{Path.Combine(connectionParameters.RootFolder ?? "", processorParameters.SubFolder ?? "", fileName)}".Replace("\\","/").Replace("//","/");
            // var path = Path.Combine(connectionParameters.RootFolder ?? "", processorParameters.SubFolder ?? "", fileName);
            using (var client = connectionParameters.CreateConnectionInfo())
            {
                var stream = new MemoryStream();
                client.Files.UploadAsync(new CommitInfo(path), stream).Wait();
            }
            using (var client = connectionParameters.CreateConnectionInfo())
                client.Files.DeleteV2Async(path).Wait();
        }
    }
}