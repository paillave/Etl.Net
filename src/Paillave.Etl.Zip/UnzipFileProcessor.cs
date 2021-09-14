using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.FileSystemGlobbing;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Paillave.Etl.Zip
{
    public class UnzipFileProcessorParams
    {
        public string Password { get; set; }
        public string FileNamePattern { get; set; }
    }
    public class UnzippedFileValueMetadata : FileValueMetadataBase, IFileValueWithDestinationMetadata
    {
        public string ParentFileName { get; set; }
        public IFileValueMetadata ParentFileMetadata { get; set; }
        public Dictionary<string, IEnumerable<Destination>> Destinations { get; set; }
    }

    public class UnzipFileProcessor : FileValueProcessorBase<object, UnzipFileProcessorParams>
    {
        public UnzipFileProcessor(string code, string name, string connectionName, object connectionParameters, UnzipFileProcessorParams processorParameters) : base(code, name, connectionName, connectionParameters, processorParameters)
        {
        }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override void Process(IFileValue fileValue, object connectionParameters, UnzipFileProcessorParams processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var destinations = (fileValue.Metadata as IFileValueWithDestinationMetadata)?.Destinations;
            if (cancellationToken.IsCancellationRequested) return;
            using (var zf = new ZipFile(fileValue.GetContent()))
            {
                var searchPattern = string.IsNullOrEmpty(processorParameters.FileNamePattern) ? "*" : processorParameters.FileNamePattern;
                var matcher = new Matcher().AddInclude(searchPattern);

                if (!String.IsNullOrEmpty(processorParameters.Password))
                    zf.Password = processorParameters.Password;
                var fileNames = zf.OfType<ZipEntry>().Where(i => i.IsFile && matcher.Match(Path.GetFileName(i.Name)).HasMatches).Select(i => i.Name).ToHashSet();
                foreach (ZipEntry zipEntry in zf)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    if (zipEntry.IsFile && matcher.Match(Path.GetFileName(zipEntry.Name)).HasMatches)
                    {
                        MemoryStream outputStream = new MemoryStream();
                        using (var zipStream = zf.GetInputStream(zipEntry))
                            zipStream.CopyTo(outputStream, 4096);
                        outputStream.Seek(0, SeekOrigin.Begin);
                        push(new UnzippedFileValue<UnzippedFileValueMetadata>(outputStream, zipEntry.Name, new UnzippedFileValueMetadata
                        {
                            ParentFileName = fileValue.Name,
                            ParentFileMetadata = fileValue.Metadata,
                            Destinations = destinations
                        }, fileValue, fileNames, zipEntry.Name));
                    }
                }
            }
        }
        protected override void Test(object connectionParameters, UnzipFileProcessorParams processorParameters) { }
    }
}
