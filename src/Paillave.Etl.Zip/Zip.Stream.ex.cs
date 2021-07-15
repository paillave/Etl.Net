using Paillave.Etl.Core;

namespace Paillave.Etl.Zip
{
    public static class ZipEx
    {
        public static IStream<IFileValue> CrossApplyZipFiles(this IStream<IFileValue> stream, string name, string pattern = "*", string password = null, bool noParallelisation = false)
                => stream.CrossApply<IFileValue, IFileValue>(name, new UnzipFileProcessorValuesProvider(new UnzipFileProcessorParams
                {
                    FileNamePattern = pattern,
                    Password = password
                }), noParallelisation);
    }
}