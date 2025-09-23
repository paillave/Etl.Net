using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Renci.SshNet;

namespace Paillave.Etl.Sftp
{
    public class SftpFileValueProcessor : FileValueProcessorBase<SftpAdapterConnectionParameters, SftpAdapterProcessorParameters>
    {
        public SftpFileValueProcessor(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters processorParameters)
            : base(code, name, connectionName, connectionParameters, processorParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Process(IFileValue fileValue, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken)
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (processorParameters.SubFolder ?? "") : StringEx.ConcatenatePath(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
            using var stream = fileValue.Get(processorParameters.UseStreamCopy);
            byte[] fileContents;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                fileContents = ms.ToArray();
            }
            ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => UploadSingleTime(connectionParameters, fileContents, StringEx.ConcatenatePath(folder, fileValue.Name)));
            push(fileValue);
        }
        private void UploadSingleTime(SftpAdapterConnectionParameters connectionParameters, byte[] fileContents, string filePath)
        {
            var connectionInfo = connectionParameters.CreateConnectionInfo();
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                if (client.Exists(filePath))
                    client.DeleteFile(filePath);
                client.WriteAllBytes(filePath, fileContents);
            }
        }
        protected override void Test(SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters processorParameters)
        {
            var fileName = Guid.NewGuid().ToString();
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (processorParameters.SubFolder ?? "") : StringEx.ConcatenatePath(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
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

                client.WriteAllBytes(StringEx.ConcatenatePath(folder, fileName), fileContents);
            }
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                client.DeleteFile(StringEx.ConcatenatePath(folder, fileName));
            }
        }
    }
}