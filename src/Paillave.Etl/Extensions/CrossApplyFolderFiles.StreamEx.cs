using Paillave.Etl.Core.Streams;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.ValuesProviders;
using System;
using System.IO;
using System.Linq;
using SystemIO = System.IO;

namespace Paillave.Etl.Extensions
{
    public static class CrossApplyFolderFilesEx
    {
        public static IStream<LocalFilesValue> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, string pattern = "*", bool recursive = false, bool noParallelisation = false)
        {
            var valuesProvider = new LocalFilesValuesProvider();
            return stream.CrossApply<TIn, LocalFilesValue>(name, (inputValue, push) =>
            {
                var args = new LocalFilesValuesProviderArgs
                {
                    RootFolder = getFolderPath(inputValue),
                    SearchPattern = pattern,
                    Recursive = recursive
                };
                valuesProvider.PushValues(push, args);
            }, noParallelisation);
        }
        public static IStream<LocalFilesValue> CrossApplyFolderFiles(this IStream<string> stream, string name, string pattern = "*", bool recursive = false, bool noParallelisation = false)
        {
            var valuesProvider = new LocalFilesValuesProvider();
            return stream.CrossApply<string, LocalFilesValue>(name, (inputValue, push) =>
            {
                var args = new LocalFilesValuesProviderArgs
                {
                    RootFolder = inputValue,
                    SearchPattern = pattern,
                    Recursive = recursive
                };
                valuesProvider.PushValues(push, args);
            }, noParallelisation);
        }
        public static IStream<LocalFilesValue> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, bool recursive = false, bool noParallelisation = false)
        {
            var valuesProvider = new LocalFilesValuesProvider();
            return stream.CrossApply<TIn, LocalFilesValue>(name, (inputValue, push) =>
            {
                var args = new LocalFilesValuesProviderArgs
                {
                    RootFolder = getFolderPath(inputValue),
                    SearchPattern = getSearchPattern(inputValue),
                    Recursive = recursive
                };
                valuesProvider.PushValues(push, args);
            }, noParallelisation);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<LocalFilesValue, TIn, TOut> selector, string pattern = "*", bool recursive = false, bool noParallelisation = false)
        {
            var valuesProvider = new LocalFilesValuesProvider();
            return stream.CrossApply<TIn, TIn, LocalFilesValue, TOut>(name, (inputValue, push) =>
            {
                var args = new LocalFilesValuesProviderArgs
                {
                    RootFolder = getFolderPath(inputValue),
                    SearchPattern = pattern,
                    Recursive = recursive
                };
                valuesProvider.PushValues(push, args);
            }, i => i, selector, noParallelisation);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TOut>(this IStream<string> stream, string name, Func<LocalFilesValue, string, TOut> selector, string pattern = "*", bool recursive = false, bool noParallelisation = false)
        {
            var valuesProvider = new LocalFilesValuesProvider();
            return stream.CrossApply<string, string, LocalFilesValue, TOut>(name, (inputValue, push) =>
            {
                var args = new LocalFilesValuesProviderArgs
                {
                    RootFolder = inputValue,
                    SearchPattern = pattern,
                    Recursive = recursive
                };
                valuesProvider.PushValues(push, args);
            }, i => i, selector, noParallelisation);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, Func<LocalFilesValue, TIn, TOut> selector, bool recursive = false, bool noParallelisation = false)
        {
            var valuesProvider = new LocalFilesValuesProvider();
            return stream.CrossApply(name, (inputValue, push) =>
            {
                var args = new LocalFilesValuesProviderArgs
                {
                    RootFolder = getFolderPath(inputValue),
                    SearchPattern = getSearchPattern(inputValue),
                    Recursive = recursive
                };
                valuesProvider.PushValues(push, args);
            }, i => i, selector, noParallelisation);
        }
    }
}
