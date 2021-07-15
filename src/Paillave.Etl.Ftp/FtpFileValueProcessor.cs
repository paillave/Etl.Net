using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using FluentFTP;

namespace Paillave.Etl.Ftp
{
    public class FtpFileValueProcessor : FileValueProcessorBase<FtpAdapterConnectionParameters, FtpAdapterProcessorParameters>
    {
        public FtpFileValueProcessor(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters processorParameters)
            : base(code, name, connectionName, connectionParameters, processorParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Process(IFileValue fileValue, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (processorParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");

            // var folder = Path.Combine(connectionParameters.RootFolder ?? "", processorParameters.SubFolder ?? "");
            var filePath = Path.Combine(folder, fileValue.Name);
            var stream = fileValue.GetContent();
            byte[] fileContents;
            stream.Position = 0;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                fileContents = ms.ToArray();
            }
            ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => UploadSingleTime(connectionParameters, fileContents, filePath));
            push(fileValue);
        }
        private void UploadSingleTime(FtpAdapterConnectionParameters connectionParameters, byte[] fileContents, string filePath)
        {
            using (FtpClient client = connectionParameters.CreateFtpClient())
                client.Upload(fileContents, filePath);
        }

        protected override void Test(FtpAdapterConnectionParameters connectionParameters, FtpAdapterProcessorParameters processorParameters)
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (processorParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
            var testFilePath = Path.Combine(folder, Guid.NewGuid().ToString());
            using (FtpClient client = connectionParameters.CreateFtpClient())
            {
                var stream = new MemoryStream();
                byte[] fileContents;
                stream.Position = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    fileContents = ms.ToArray();
                }
                client.Upload(stream, testFilePath);
                client.DeleteFile(testFilePath);
            }
        }
    }
}