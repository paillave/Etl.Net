using System;
using System.Linq;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Paillave.Etl.Core;
using System.Threading;
using Microsoft.Extensions.FileSystemGlobbing;

namespace Paillave.Etl.Zip
{
    public class UnzipFileProcessorValuesProvider : ValuesProviderBase<IFileValue, IFileValue>
    {
        private UnzipFileProcessorParams _args;
        public UnzipFileProcessorValuesProvider(UnzipFileProcessorParams args)
            => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        public override void PushValues(IFileValue input, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            var destinations = (input.Metadata as IFileValueWithDestinationMetadata)?.Destinations;
            if (cancellationToken.IsCancellationRequested) return;
            using var stream = input.Get(_args.UseStreamCopy);
            context.AddUnderlyingDisposables(stream);
            using var zf = new ZipFile(stream);
            var searchPattern = string.IsNullOrEmpty(_args.FileNamePattern) ? "*" : _args.FileNamePattern;
            var matcher = new Matcher().AddInclude(searchPattern);

            if (!String.IsNullOrEmpty(_args.Password))
                zf.Password = _args.Password;
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
                        ParentFileName = input.Name,
                        ParentFileMetadata = input.Metadata,
                        Destinations = destinations,
                        ConnectorCode = input.Metadata.ConnectorCode,
                        ConnectionName = input.Metadata.ConnectionName,
                        ConnectorName = input.Metadata.ConnectorName
                    }, input, fileNames, zipEntry.Name));
                }
            }
        }
    }
}
