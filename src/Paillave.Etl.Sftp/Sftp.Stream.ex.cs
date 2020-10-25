using Paillave.Etl.Core.Streams;
using Paillave.Etl.Sftp;
using System;
using System.IO;
using System.Linq;
using SystemIO = System.IO;
using Paillave.Etl;
using Paillave.Etl.Extensions;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.Sftp
{
    public static class SftpEx
    {
        #region CrossApplySftpFiles
        public static IStream<IFileValue> CrossApplySftpFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern = null, SftpConnectionInfo connectionInfo = null, bool noParallelisation = false)
            => stream.CrossApply<TIn, IFileValue>(name, new SftpFilesValuesProvider<TIn, IFileValue>(new SftpFilesValuesProviderArgs<TIn, IFileValue>
            {
                Stream = stream,
                GetFolderPath = getFolderPath,
                GetSearchPattern = getSearchPattern,
                ConnectionInfo = connectionInfo,
                GetResult = (i, j) => i
            }), noParallelisation);
        public static IStream<IFileValue> CrossApplySftpFiles(this IStream<string> stream, string name, string searchPattern = null, SftpConnectionInfo connectionInfo = null, bool noParallelisation = false)
            => stream.CrossApply<string, IFileValue>(name, new SftpFilesValuesProvider<string, IFileValue>(new SftpFilesValuesProviderArgs<string, IFileValue>
            {
                Stream = stream,
                GetFolderPath = i => i,
                GetSearchPattern = i => searchPattern,
                ConnectionInfo = connectionInfo,
                GetResult = (i, j) => i
            }), noParallelisation);
        public static IStream<TOut> CrossApplySftpFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<IFileValue, TIn, TOut> selector, Func<TIn, string> getSearchPattern = null, SftpConnectionInfo connectionInfo = null, bool noParallelisation = false)
            => stream.CrossApply<TIn, TOut>(name, new SftpFilesValuesProvider<TIn, TOut>(new SftpFilesValuesProviderArgs<TIn, TOut>
            {
                Stream = stream,
                GetFolderPath = getFolderPath,
                GetSearchPattern = getSearchPattern,
                ConnectionInfo = connectionInfo,
                GetResult = selector
            }), noParallelisation);
        public static IStream<TOut> CrossApplySftpFiles<TOut>(this IStream<string> stream, string name, Func<IFileValue, string, TOut> selector, string searchPattern = null, SftpConnectionInfo connectionInfo = null, bool noParallelisation = false)
            => stream.CrossApply<string, TOut>(name, new SftpFilesValuesProvider<string, TOut>(new SftpFilesValuesProviderArgs<string, TOut>
            {
                Stream = stream,
                GetFolderPath = i => i,
                ConnectionInfo = connectionInfo,
                GetSearchPattern = i => searchPattern,
                GetResult = selector
            }), noParallelisation);
        #endregion

        #region ToSftpFile
        public static IStream<IFileValue> ToSftpFile<TParams>(this IStream<IFileValue> stream, string name, ISingleStream<TParams> paramsStream, Func<TParams, string> getOutputFolder)
        {
            return new WriteToSftpFileStreamNode<TParams>(name, new WriteToSftpFileArgs<TParams>
            {
                GetOutputFolder = getOutputFolder,
                GetConnectionInfo = i => stream.SourceNode.ExecutionContext.DependencyResolver.Resolve<SftpConnectionInfo>(),
                ParamStream = paramsStream,
                Stream = stream
            }).Output;
        }
        #endregion
    }
}
