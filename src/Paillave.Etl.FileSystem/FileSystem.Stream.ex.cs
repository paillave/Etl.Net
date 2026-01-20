using Paillave.Etl.Core;
using System;

namespace Paillave.Etl.FileSystem;

public static class FileSystemEx
{
    public static IStream<IFileValue> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, string pattern = "*", bool recursive = false, bool noParallelisation = false)
            => stream.CrossApply<TIn, IFileValue>(name, new FileSystemValuesProvider<TIn, IFileValue>(new FileSystemValuesProviderArgs<TIn, IFileValue>
            {
                GetFolderPath = getFolderPath,
                GetSearchPattern = i => pattern,
                Recursive = recursive,
                GetResult = (i, j) => i
            }), noParallelisation);
    public static IStream<IFileValue> CrossApplyFolderFiles(this IStream<string> stream, string name, string pattern = "*", bool recursive = false, bool noParallelisation = false)
            => stream.CrossApply<string, IFileValue>(name, new FileSystemValuesProvider<string, IFileValue>(new FileSystemValuesProviderArgs<string, IFileValue>
            {
                GetFolderPath = i => i,
                GetSearchPattern = i => pattern,
                Recursive = recursive,
                GetResult = (i, j) => i
            }), noParallelisation);
    public static IStream<IFileValue> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, bool recursive = false, bool noParallelisation = false)
            => stream.CrossApply<TIn, IFileValue>(name, new FileSystemValuesProvider<TIn, IFileValue>(new FileSystemValuesProviderArgs<TIn, IFileValue>
            {
                GetFolderPath = getFolderPath,
                GetSearchPattern = getSearchPattern,
                Recursive = recursive,
                GetResult = (i, j) => i
            }), noParallelisation);
    public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<IFileValue, TIn, TOut> selector, string pattern = "*", bool recursive = false, bool noParallelisation = false)
            => stream.CrossApply<TIn, TOut>(name, new FileSystemValuesProvider<TIn, TOut>(new FileSystemValuesProviderArgs<TIn, TOut>
            {
                GetFolderPath = getFolderPath,
                GetSearchPattern = i => pattern,
                Recursive = recursive,
                GetResult = selector
            }), noParallelisation);
    public static IStream<TOut> CrossApplyFolderFiles<TOut>(this IStream<string> stream, string name, Func<IFileValue, string, TOut> selector, string pattern = "*", bool recursive = false, bool noParallelisation = false)
            => stream.CrossApply<string, TOut>(name, new FileSystemValuesProvider<string, TOut>(new FileSystemValuesProviderArgs<string, TOut>
            {
                GetFolderPath = i => i,
                GetSearchPattern = i => pattern,
                Recursive = recursive,
                GetResult = selector
            }), noParallelisation);
    public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, Func<IFileValue, TIn, TOut> selector, bool recursive = false, bool noParallelisation = false)
            => stream.CrossApply(name, new FileSystemValuesProvider<TIn, TOut>(new FileSystemValuesProviderArgs<TIn, TOut>
            {
                GetFolderPath = getFolderPath,
                GetSearchPattern = getSearchPattern,
                Recursive = recursive,
                GetResult = selector
            }), noParallelisation);
    [Obsolete("KISS & YAGNI")]
    public static IStream<IFileValue> WriteToFile(this IStream<IFileValue> stream, string name,
    ISingleStream<string> outputFilePathStream, bool useStreamCopy = true)
    {
        return new WriteToFileStreamNode<string>(name, new WriteToFileArgs<string>
        {
            GetOutputFilePath = i => i,
            ParamStream = outputFilePathStream,
            Stream = stream,
            UseStreamCopy = useStreamCopy
        }).Output;
    }
    public static IStream<IFileValue> WriteToFile(this IStream<IFileValue> stream, string name, Func<IFileValue, string> getOutputFilePath, bool useStreamCopy = true)
    {
        return new WriteToFileStreamNode(name, new WriteToFileArgs
        {
            Stream = stream,
            GetOutputFilePath = getOutputFilePath,
            UseStreamCopy = useStreamCopy
        }).Output;
    }
    [Obsolete("KISS & YAGNI")]
    public static IStream<IFileValue> WriteToFile<TParam>(this IStream<IFileValue> stream, string name, ISingleStream<TParam> outputFilePathStream, Func<TParam, string> getOutputFilePath, bool useStreamCopy = true)
    {
        return new WriteToFileStreamNode<TParam>(name, new WriteToFileArgs<TParam>
        {
            GetOutputFilePath = getOutputFilePath,
            ParamStream = outputFilePathStream,
            Stream = stream,
            UseStreamCopy = useStreamCopy
        }).Output;
    }
}
