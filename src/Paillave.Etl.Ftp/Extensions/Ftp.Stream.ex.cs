using Paillave.Etl.Core.Streams;
using Paillave.Etl.Ftp;
using Paillave.Etl.Ftp.StreamNodes;
using Paillave.Etl.Ftp.ValuesProviders;
using System;
using System.IO;
using System.Linq;
using SystemIO = System.IO;
using Paillave.Etl;
using Paillave.Etl.Extensions;

namespace Paillave.Etl.Ftp.Extensions
{
    public static class FtpEx
    {
        #region CrossApplyFtpFiles
        public static IStream<FtpFilesValue> CrossApplyFtpFiles<TIn>(this IStream<TIn> stream, string name, ISingleStream<FtpConnectionInfo> connectionInfoS, Func<TIn, string> getFolderPath, bool noParallelisation = false)
        {
            var valuesProvider = new FtpFilesValuesProvider();
            return stream.CrossApply<TIn, FtpConnectionInfo, FtpFilesValuesProviderArgs, FtpFilesValue, FtpFilesValue>(name, connectionInfoS, valuesProvider.PushValues, (i, j) => new FtpFilesValuesProviderArgs { Path = getFolderPath(i) }, (i, j, k) => i, noParallelisation);
        }
        public static IStream<FtpFilesValue> CrossApplyFtpFiles(this IStream<string> stream, string name, ISingleStream<FtpConnectionInfo> connectionInfoS, bool noParallelisation = false)
        {
            var valuesProvider = new FtpFilesValuesProvider();
            return stream.CrossApply<string, FtpConnectionInfo, FtpFilesValuesProviderArgs, FtpFilesValue, FtpFilesValue>(name, connectionInfoS, valuesProvider.PushValues, (i, j) => new FtpFilesValuesProviderArgs { Path = i }, (i, j, k) => i, noParallelisation);
        }
        public static IStream<TOut> CrossApplyFtpFiles<TIn, TOut>(this IStream<TIn> stream, string name, ISingleStream<FtpConnectionInfo> connectionInfoS, Func<TIn, string> getFolderPath, Func<FtpFilesValue, TIn, FtpConnectionInfo, TOut> selector, bool noParallelisation = false)
        {
            var valuesProvider = new FtpFilesValuesProvider();
            return stream.CrossApply<TIn, FtpConnectionInfo, FtpFilesValuesProviderArgs, FtpFilesValue, TOut>(name, connectionInfoS, valuesProvider.PushValues, (i, j) => new FtpFilesValuesProviderArgs { Path = getFolderPath(i) }, selector, noParallelisation);
        }
        public static IStream<TOut> CrossApplyFtpFiles<TOut>(this IStream<string> stream, string name, ISingleStream<FtpConnectionInfo> connectionInfoS, Func<FtpFilesValue, string, FtpConnectionInfo, TOut> selector, bool noParallelisation = false)
        {
            var valuesProvider = new FtpFilesValuesProvider();
            return stream.CrossApply<string, FtpConnectionInfo, FtpFilesValuesProviderArgs, FtpFilesValue, TOut>(name, connectionInfoS, valuesProvider.PushValues, (i, j) => new FtpFilesValuesProviderArgs { Path = i }, selector, noParallelisation);
        }
        #endregion

        #region ToFtpFile
        public static IStream<Stream> ToFtpFile<TParams>(this IStream<Stream> stream, string name, ISingleStream<TParams> paramsStream, Func<TParams, string> getOutputFilePath, Func<TParams, FtpConnectionInfo> getConnectionInfo)
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
