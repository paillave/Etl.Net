using Paillave.Etl.Core;

namespace Paillave.Etl.Zip;

public static class ZipEx
{
    public static IStream<IFileValue> CrossApplyZipFiles(
        this IStream<IFileValue> stream,
        string name, string pattern = "*",
        string password = null,
        bool noParallelisation = false,
        bool useStreamCopy = true
    )
        => stream.CrossApply<IFileValue, IFileValue>(name, new UnzipFileProcessorValuesProvider(new UnzipFileProcessorParams
        {
            FileNamePattern = pattern,
            Password = password,
            UseStreamCopy = useStreamCopy
        }), noParallelisation);

    public static IStream<IFileValue> SelectZip(
        this IStream<IFileValue> stream,
        string name,
        string fileName = null,
        string password = null,
        bool noParallelisation = false
    )
        => stream.CrossApply<IFileValue, IFileValue>(name, new ZipFileProcessorValuesProvider(new ZipFileProcessorParams
        {
            FileName = fileName,
            Password = password
        }), noParallelisation);
}
