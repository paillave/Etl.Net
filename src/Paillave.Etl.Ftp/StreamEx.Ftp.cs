using Paillave.Etl.Core.Streams;
using Paillave.Etl.Ftp;
using Paillave.Etl.Ftp.StreamNodes;
using Paillave.Etl.Ftp.ValuesProviders;
using System;
using System.IO;
using System.Linq;
using SystemIO = System.IO;

namespace Paillave.Etl
{
    public static class StreamExFtp
    {
        #region CrossApplyFtpFiles
        public static IStream<FtpFilesValue> CrossApplyFtpFiles<TIn>(this IStream<TIn> stream, string name, IStream<FtpConnectionInfo> connectionInfoS, Func<TIn, string> getFolderPath)
        {
            return stream.CrossApply(name, connectionInfoS, new FtpFilesValuesProvider(), (i, j) => new FtpFilesValuesProviderArgs { Path = getFolderPath(i) }, (i, j, k) => i);
        }
        public static IStream<FtpFilesValue> CrossApplyFtpFiles(this IStream<string> stream, string name, IStream<FtpConnectionInfo> connectionInfoS)
        {
            return stream.CrossApply(name, connectionInfoS, new FtpFilesValuesProvider(), (i, j) => new FtpFilesValuesProviderArgs { Path = i }, (i, j, k) => i);
        }
        public static IStream<TOut> CrossApplyFtpFiles<TIn, TOut>(this IStream<TIn> stream, string name, IStream<FtpConnectionInfo> connectionInfoS, Func<TIn, string> getFolderPath, Func<FtpFilesValue, TIn, FtpConnectionInfo, TOut> selector)
        {
            return stream.CrossApply(name, connectionInfoS, new FtpFilesValuesProvider(), (i, j) => new FtpFilesValuesProviderArgs { Path = getFolderPath(i) }, selector);
        }
        public static IStream<TOut> CrossApplyFtpFiles<TOut>(this IStream<string> stream, string name, IStream<FtpConnectionInfo> connectionInfoS, Func<FtpFilesValue, string, FtpConnectionInfo, TOut> selector)
        {
            return stream.CrossApply(name, connectionInfoS, new FtpFilesValuesProvider(), (i, j) => new FtpFilesValuesProviderArgs { Path = i }, selector);
        }
        #endregion

        #region ToFtpFile
        public static IStream<Stream> ToFtpFile<TParams>(this IStream<Stream> stream, string name, IStream<TParams> paramsStream, Func<TParams, string> getOutputFilePath, Func<TParams, FtpConnectionInfo> getConnectionInfo)
        {
            return new ToFtpFileStreamNode<TParams>(name, new ToFtpFileArgs<TParams>
            {
                GetOutputFilePath = getOutputFilePath,
                GetConnectionInfo = getConnectionInfo,
                ParamStream = paramsStream,
                Stream = stream
            }).Output;
        }
        #endregion
    }
}
