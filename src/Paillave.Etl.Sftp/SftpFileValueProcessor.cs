using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using Renci.SshNet;

namespace Paillave.Etl.Sftp
{
    public class SftpFileValueProcessor : FileValueProcessorBase<SftpAdapterConnectionParameters, SftpAdapterProcessorParameters>
    {
        public SftpFileValueProcessor(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters processorParameters)
            : base(code, name, connectionName, connectionParameters, processorParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Process(IFileValue fileValue, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var folder = Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
            var connectionInfo = connectionParameters.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                var stream = fileValue.GetContent();
                byte[] fileContents;
                stream.Position = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    fileContents = ms.ToArray();
                }

                client.WriteAllBytes(Path.Combine(folder, fileValue.Name), fileContents);
            }
            push(fileValue);
        }

        protected override void Test(SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters processorParameters)
        {
            var fileName = Guid.NewGuid().ToString();
            var folder = Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
            var connectionInfo = connectionParameters.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                var stream = new MemoryStream();
                byte[] fileContents;
                stream.Position = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    fileContents = ms.ToArray();
                }

                client.WriteAllBytes(Path.Combine(folder, fileName), fileContents);
            }
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                client.DeleteFile(Path.Combine(folder, fileName));
            }
        }
    }
}