using System;
using System.IO;
using System.Threading;
using FluentFtp;
using Paillave.Etl.Core;

namespace Paillave.Etl.HttpExtension;
{
    public class HttpFileValueProcessor
        : FileValueProcessorBase<HttpAdapterConnectionParameters, HttpAdapterProcessorParameters>
    {
        public HttpFileValueProcessor(
            string code,
            string name,
            string connectionName,
            HttpAdapterConnectionParameters connectionParameters,
            HttpAdapterProcessorParameters processorParameters
        )
            : base(code, name, connectionName, connectionParameters, processorParameters) { }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override void Process(
            IFileValue fileValue,
            HttpAdapterConnectionParameters connectionParameters,
            HttpAdapterProcessorParameters processorParameters,
            Action<IFileValue> push,
            CancellationToken cancellationToken,
            IExecutionContext context
        )
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder)
                ? (processorParameters.SubFolder ?? "")
                : StringEx.ConcatenatePath(
                    connectionParameters.RootFolder,
                    processorParameters.SubFolder ?? ""
                );

            // var folder = StringEx.ConcatenatePath(connectionParameters.RootFolder ?? "", processorParameters.SubFolder ?? "");
            var filePath = StringEx.ConcatenatePath(folder, fileValue.Name);
            filePath = SmartFormat.Smart.Format(filePath.Replace(@"\", @"\\"), fileValue.Metadata);
            using var stream = fileValue.Get(processorParameters.UseStreamCopy);
            byte[] fileContents;
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                fileContents = ms.ToArray();
            }
            ActionRunner.TryExecute(
                connectionParameters.MaxAttempts,
                () =>
                    UploadSingleTime(
                        connectionParameters,
                        fileContents,
                        filePath,
                        processorParameters.BuildMissingSubFolders
                    )
            );
            push(fileValue);
        }

        private void UploadSingleTime(
            HttpAdapterConnectionParameters connectionParameters,
            byte[] fileContents,
            string filePath,
            bool buildMissingSubFolders
        )
        {
            using (HttpClient client = connectionParameters.CreateHttpClient())
            {
                if (buildMissingSubFolders)
                {
                    var directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrWhiteSpace(directory))
                        client.CreateDirectory(directory);
                }

                client.UploadBytes(fileContents, filePath);
            }
        }

        protected override void Test(
            HttpAdapterConnectionParameters connectionParameters,
            HttpAdapterProcessorParameters processorParameters
        )
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder)
                ? (processorParameters.SubFolder ?? "")
                : StringEx.ConcatenatePath(
                    connectionParameters.RootFolder,
                    processorParameters.SubFolder ?? ""
                );
            var testFilePath = StringEx.ConcatenatePath(folder, Guid.NewGuid().ToString());
            using (HttpClient client = connectionParameters.CreateHttpClient())
            {
                var stream = new MemoryStream();
                byte[] fileContents;
                stream.Position = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    fileContents = ms.ToArray();
                }
                client.UploadStream(stream, testFilePath);
                client.DeleteFile(testFilePath);
            }
        }
    }
}
