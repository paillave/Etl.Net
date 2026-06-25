using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Paillave.Etl.Core;
using System.Threading;

namespace Paillave.Etl.Zip;

public class ZipFileProcessorParams
{
    [Sensitive]
    public string Password { get; set; }
    public bool UseStreamCopy { get; set; } = true;
    public string FileName { get; set; }
}
public class ZipFileProcessorValuesProvider(ZipFileProcessorParams args) : ValuesProviderBase<IFileValue, IFileValue>
{
    private readonly ZipFileProcessorParams _args = args;

    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    public override void PushValues(IFileValue input, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
        => PushValues(input, push, cancellationToken);
    public void PushValues(IFileValue input, Action<IFileValue> push, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        using var stream = input.Get(_args.UseStreamCopy);
        var ms = new MemoryStream();
        var zipFileName = Path.ChangeExtension(
            string.IsNullOrEmpty(_args.FileName) ? Path.GetFileNameWithoutExtension(input.Name) : _args.FileName,
            ".zip");
        using (var zipStream = new ZipOutputStream(ms))
        {
            zipStream.IsStreamOwner = false;
            if (!string.IsNullOrEmpty(_args.Password))
                zipStream.Password = _args.Password;

            var zipEntry = new ZipEntry(ZipEntry.CleanName(input.Name))
            {
                DateTime = DateTime.Now,
                IsUnicodeText = true
            };

            zipStream.PutNextEntry(zipEntry);
            stream.CopyTo(zipStream);
            zipStream.CloseEntry();
        }
        ms.Seek(0, SeekOrigin.Begin);
        push(new ZippedFileValue(ms, zipFileName, input));
    }
}
