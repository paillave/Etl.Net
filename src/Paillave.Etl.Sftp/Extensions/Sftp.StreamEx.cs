using Paillave.Etl.Core.Streams;
using Paillave.Etl.Sftp;
using Paillave.Etl.Sftp.StreamNodes;
using Paillave.Etl.Sftp.ValuesProviders;
using System;
using System.IO;
using System.Linq;
using SystemIO = System.IO;
using Paillave.Etl;
using Paillave.Etl.Extensions;

namespace Paillave.Etl.Sftp.Extensions
{
    public static class SftpEx
    {
        #region CrossApplySftpFiles
        public static IStream<SftpFilesValue> CrossApplySftpFiles<TIn>(this IStream<TIn> stream, string name, ISingleStream<SftpConnectionInfo> connectionInfoS, Func<TIn, string> getFolderPath, bool noParallelisation = false)
        {
            var valuesProvider = new SftpFilesValuesProvider();
            return stream.CrossApply<TIn, SftpConnectionInfo, SftpFilesValuesProviderArgs, SftpFilesValue, SftpFilesValue>(name, connectionInfoS, valuesProvider.PushValues, (i, j) => new SftpFilesValuesProviderArgs { Path = getFolderPath(i) }, (i, j, k) => i, noParallelisation);
        }
        public static IStream<SftpFilesValue> CrossApplySftpFiles(this IStream<string> stream, string name, ISingleStream<SftpConnectionInfo> connectionInfoS, bool noParallelisation = false)
        {
            var valuesProvider = new SftpFilesValuesProvider();
            return stream.CrossApply<string, SftpConnectionInfo, SftpFilesValuesProviderArgs, SftpFilesValue, SftpFilesValue>(name, connectionInfoS, valuesProvider.PushValues, (i, j) => new SftpFilesValuesProviderArgs { Path = i }, (i, j, k) => i, noParallelisation);
        }
        public static IStream<TOut> CrossApplySftpFiles<TIn, TOut>(this IStream<TIn> stream, string name, ISingleStream<SftpConnectionInfo> connectionInfoS, Func<TIn, string> getFolderPath, Func<SftpFilesValue, TIn, SftpConnectionInfo, TOut> selector, bool noParallelisation = false)
        {
            var valuesProvider = new SftpFilesValuesProvider();
            return stream.CrossApply<TIn, SftpConnectionInfo, SftpFilesValuesProviderArgs, SftpFilesValue, TOut>(name, connectionInfoS, valuesProvider.PushValues, (i, j) => new SftpFilesValuesProviderArgs { Path = getFolderPath(i) }, selector, noParallelisation);
        }
        public static IStream<TOut> CrossApplySftpFiles<TOut>(this IStream<string> stream, string name, ISingleStream<SftpConnectionInfo> connectionInfoS, Func<SftpFilesValue, string, SftpConnectionInfo, TOut> selector, bool noParallelisation = false)
        {
            var valuesProvider = new SftpFilesValuesProvider();
            return stream.CrossApply<string, SftpConnectionInfo, SftpFilesValuesProviderArgs, SftpFilesValue, TOut>(name, connectionInfoS, valuesProvider.PushValues, (i, j) => new SftpFilesValuesProviderArgs { Path = i }, selector, noParallelisation);
        }
        #endregion

        #region ToSftpFile
        public static IStream<Stream> ToSftpFile<TParams>(this IStream<Stream> stream, string name, ISingleStream<TParams> paramsStream, Func<TParams, string> getOutputFilePath, Func<TParams, SftpConnectionInfo> getConnectionInfo)
        {
            return new ToSftpFileStreamNode<TParams>(name, new ToSftpFileArgs<TParams>
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
