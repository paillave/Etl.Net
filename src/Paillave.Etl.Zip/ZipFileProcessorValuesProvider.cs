using System;
using System.Linq;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Paillave.Etl.Core;
using System.Threading;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;

namespace Paillave.Etl.Zip;

public class ZipFileProcessorParams
{
    public string Password { get; set; }
    public bool UseStreamCopy { get; set; } = true;
}
public class ZippedFileValueMetadata : FileValueMetadataBase, IFileValueWithDestinationMetadata
{
    public IFileValueMetadata ParentFileMetadata { get; set; }
    public Dictionary<string, IEnumerable<Destination>> Destinations { get; set; }
}
public class ZipFileProcessorValuesProvider : ValuesProviderBase<IFileValue, IFileValue>
{
    private ZipFileProcessorParams _args;
    public ZipFileProcessorValuesProvider(ZipFileProcessorParams args)
        => _args = args;
    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    public override void PushValues(IFileValue input, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
    {
        var destinations = (input.Metadata as IFileValueWithDestinationMetadata)?.Destinations;
        if (cancellationToken.IsCancellationRequested) return;
        using var stream = input.Get(_args.UseStreamCopy);
        var ms = new MemoryStream();
        var fileName = $"{input.Name}.zip";
        using (ZipOutputStream zipStream = new ZipOutputStream(ms))
        {
            if (!String.IsNullOrEmpty(_args.Password))
                zipStream.Password = _args.Password;

            var zipEntry = new ZipEntry(fileName)
            {
                DateTime = DateTime.Now,
                IsUnicodeText = true
            };

            zipStream.PutNextEntry(zipEntry);
            stream.CopyTo(zipStream);
            zipStream.CloseEntry();
        }
        ms.Seek(0, SeekOrigin.Begin);
        push(new ZippedFileValue<ZippedFileValueMetadata>(ms, input.Name, new ZippedFileValueMetadata
        {
            ParentFileMetadata = input.Metadata,
            Destinations = destinations
        }, input));
    }
}
