using Paillave.Etl.Core.Streams;
using Paillave.Etl.ValuesProviders;
using System;
using System.IO;
using System.Linq;
using SystemIO = System.IO;

namespace Paillave.Etl
{
    public static class StreamExTf
    {
        public static IStream<LocalFilesValue> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs
            {
                RootFolder = getFolderPath(i),
                SearchPattern = pattern
            }, (i, j) => i);
        }
        public static IStream<LocalFilesValue> CrossApplyFolderFiles(this IStream<string> stream, string name, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs
            {
                RootFolder = i,
                SearchPattern = pattern
            }, (i, j) => i);
        }
        public static IStream<LocalFilesValue> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs
            {
                RootFolder = getFolderPath(i),
                SearchPattern = getSearchPattern(i)
            }, (i, j) => i);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<LocalFilesValue, TIn, TOut> selector, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs
            {
                RootFolder = getFolderPath(i),
                SearchPattern = pattern
            }, selector);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TOut>(this IStream<string> stream, string name, Func<LocalFilesValue, string, TOut> selector, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs
            {
                RootFolder = i,
                SearchPattern = pattern
            }, selector);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, Func<LocalFilesValue, TIn, TOut> selector, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs
            {
                RootFolder = getFolderPath(i),
                SearchPattern = getSearchPattern(i)
            }, selector);
        }
    }
}
