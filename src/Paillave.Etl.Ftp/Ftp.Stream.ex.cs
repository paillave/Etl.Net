using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.Extensions;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Ftp
{
    public static class FtpEx
    {
        #region CrossApplyFtpFiles
        [Obsolete("use connectors")]
        public static IStream<IFileValue> CrossApplyFtpFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern = null, FtpConnectionInfo connectionInfo = null, bool noParallelisation = false)
                => stream.CrossApply<TIn, IFileValue>(name, new FtpFilesValuesProvider<TIn, IFileValue>(new FtpFilesValuesProviderArgs<TIn, IFileValue>
                {
                    Stream = stream,
                    GetFolderPath = getFolderPath,
                    GetSearchPattern = getSearchPattern,
                    ConnectionInfo = connectionInfo,
                    GetResult = (i, j) => i
                }), noParallelisation);
        [Obsolete("use connectors")]
        public static IStream<IFileValue> CrossApplyFtpFiles(this IStream<string> stream, string name, string searchPattern = null, FtpConnectionInfo connectionInfo = null, bool noParallelisation = false)
                => stream.CrossApply<string, IFileValue>(name, new FtpFilesValuesProvider<string, IFileValue>(new FtpFilesValuesProviderArgs<string, IFileValue>
                {
                    Stream = stream,
                    GetFolderPath = i => i,
                    GetSearchPattern = i => searchPattern,
                    ConnectionInfo = connectionInfo,
                    GetResult = (i, j) => i
                }), noParallelisation);
        [Obsolete("use connectors")]
        public static IStream<TOut> CrossApplyFtpFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<IFileValue, TIn, TOut> selector, Func<TIn, string> getSearchPattern = null, FtpConnectionInfo connectionInfo = null, bool noParallelisation = false)
                => stream.CrossApply<TIn, TOut>(name, new FtpFilesValuesProvider<TIn, TOut>(new FtpFilesValuesProviderArgs<TIn, TOut>
                {
                    Stream = stream,
                    GetFolderPath = getFolderPath,
                    GetSearchPattern = getSearchPattern,
                    ConnectionInfo = connectionInfo,
                    GetResult = selector
                }), noParallelisation);
        [Obsolete("use connectors")]
        public static IStream<TOut> CrossApplyFtpFiles<TOut>(this IStream<string> stream, string name, Func<IFileValue, string, TOut> selector, string searchPattern = null, FtpConnectionInfo connectionInfo = null, bool noParallelisation = false)
                => stream.CrossApply<string, TOut>(name, new FtpFilesValuesProvider<string, TOut>(new FtpFilesValuesProviderArgs<string, TOut>
                {
                    Stream = stream,
                    GetFolderPath = i => i,
                    ConnectionInfo = connectionInfo,
                    GetSearchPattern = i => searchPattern,
                    GetResult = selector
                }), noParallelisation);
        #endregion

        #region ToFtpFile
        [Obsolete("use connectors")]
        public static IStream<IFileValue> WriteToFtpFile<TParams>(this IStream<IFileValue> stream, string name, ISingleStream<TParams> paramsStream, Func<TParams, string> getOutputFolder, FtpConnectionInfo connectionInfo = null)
        {
            return new WriteToFtpFileStreamNode<TParams>(name, new WriteToFtpFileArgs<TParams>
            {
                GetOutputFolder = getOutputFolder,
                GetConnectionInfo = i => connectionInfo ?? stream.SourceNode.ExecutionContext.DependencyResolver.Resolve<FtpConnectionInfo>(),
                ParamStream = paramsStream,
                Stream = stream
            }).Output;
        }
        #endregion
    }
}
