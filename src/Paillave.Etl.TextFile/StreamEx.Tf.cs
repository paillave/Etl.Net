using Paillave.Etl.Core.Streams;
using Paillave.Etl.TextFile.ValuesProviders;
using Paillave.Etl.TextFile.Core;
using System;
using System.IO;
using System.Linq;
using Paillave.Etl.TextFile.StreamNodes;
using SystemIO = System.IO;

namespace Paillave.Etl
{
    public static class StreamExTf
    {
        #region CrossApplyTextFile
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<string> stream, string name, FlatFileDefinition<TOut> args, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new FlatFileValuesProvider<string, TOut, TOut>(new FlatFileValuesProviderArgs<string, TOut, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<Stream> stream, string name, FlatFileDefinition<TOut> args, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new FlatFileValuesProvider<Stream, TOut, TOut>(new FlatFileValuesProviderArgs<Stream, TOut, TOut>()
            {
                DataStreamSelector = i => i,
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TOut>(this IStream<TIn> stream, string name, FlatFileDefinition<TOut> args, Func<TIn, string> filePathSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new FlatFileValuesProvider<TIn, TOut, TOut>(new FlatFileValuesProviderArgs<TIn, TOut, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, FlatFileDefinition<TParsed> args, Func<TIn, string> filePathSelector, Func<TIn, TParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new FlatFileValuesProvider<TIn, TParsed, TOut>(new FlatFileValuesProviderArgs<TIn, TParsed, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<string> stream, string name, FlatFileDefinition<TParsed> args, Func<string, TParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new FlatFileValuesProvider<string, TParsed, TOut>(new FlatFileValuesProviderArgs<string, TParsed, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<Stream> stream, string name, FlatFileDefinition<TParsed> args, Func<TParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new FlatFileValuesProvider<Stream, TParsed, TOut>(new FlatFileValuesProviderArgs<Stream, TParsed, TOut>()
            {
                DataStreamSelector = i => i,
                Mapping = args,
                NoParallelisation = noParallelisation,
                ResultSelector = (s, o) => resultSelector(o)
            }), i => i, (i, _) => i);
        }

        public static IStream<string> CrossApplyTextFile(this IStream<string> stream, string name, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<string, string>(new TextFileValuesProviderArgs<string, string>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, (i, _) => i);
        }
        public static IStream<string> CrossApplyTextFile(this IStream<Stream> stream, string name, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<Stream, string>(new TextFileValuesProviderArgs<Stream, string>()
            {
                DataStreamSelector = i => i,
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, (i, _) => i);
        }
        public static IStream<string> CrossApplyTextFile<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> filePathSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<TIn, string>(new TextFileValuesProviderArgs<TIn, string>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                NoParallelisation = noParallelisation,
                ResultSelector = (i, o) => o
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> filePathSelector, Func<TIn, string, TOut> resultSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<TIn, TOut>(new TextFileValuesProviderArgs<TIn, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<string> stream, string name, Func<string, string, TOut> resultSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<string, TOut>(new TextFileValuesProviderArgs<string, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                NoParallelisation = noParallelisation,
                ResultSelector = resultSelector
            }), i => i, (i, _) => i);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<Stream> stream, string name, Func<string, TOut> resultSelector, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new TextFileValuesProvider<Stream, TOut>(new TextFileValuesProviderArgs<Stream, TOut>()
            {
                DataStreamSelector = i => i,
                NoParallelisation = noParallelisation,
                ResultSelector = (s, o) => resultSelector(o)
            }), i => i, (i, _) => i);
        }
        #endregion

        #region ToTextFile
        public static IStream<TIn> ToTextFile<TIn>(this IStream<TIn> stream, string name, IStream<SystemIO.Stream> resourceStream, FlatFileDefinition<TIn> mapping)
        {
            return new ToFlatFileStreamNode<TIn, IStream<TIn>>(name, new ToFlatFileArgs<TIn, IStream<TIn>>
            {
                MainStream = stream,
                Mapping = mapping,
                TargetStream = resourceStream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToTextFile<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, IStream<SystemIO.Stream> resourceStream, FlatFileDefinition<TIn> mapping)
        {
            return new ToFlatFileStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ToFlatFileArgs<TIn, ISortedStream<TIn, TKey>>
            {
                MainStream = stream,
                Mapping = mapping,
                TargetStream = resourceStream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToTextFile<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IStream<SystemIO.Stream> resourceStream, FlatFileDefinition<TIn> mapping)
        {
            return new ToFlatFileStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ToFlatFileArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                MainStream = stream,
                Mapping = mapping,
                TargetStream = resourceStream
            }).Output;
        }
        #endregion

        #region CrossApplyFolderFiles
        public static IStream<LocalFilesValue> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = pattern }, (i, j) => i);
        }
        public static IStream<LocalFilesValue> CrossApplyFolderFiles(this IStream<string> stream, string name, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = i, SearchPattern = pattern }, (i, j) => i);
        }
        public static IStream<LocalFilesValue> CrossApplyFolderFiles<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = getSearchPattern(i) }, (i, j) => i);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<LocalFilesValue, TIn, TOut> selector, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = pattern }, selector);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TOut>(this IStream<string> stream, string name, Func<LocalFilesValue, string, TOut> selector, string pattern = "*", SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = i, SearchPattern = pattern }, selector);
        }
        public static IStream<TOut> CrossApplyFolderFiles<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getFolderPath, Func<TIn, string> getSearchPattern, Func<LocalFilesValue, TIn, TOut> selector, SearchOption option = SearchOption.TopDirectoryOnly)
        {
            return stream.CrossApply(name, new LocalFilesValuesProvider(), i => new LocalFilesValuesProviderArgs { RootFolder = getFolderPath(i), SearchPattern = getSearchPattern(i) }, selector);
        }
        #endregion
    }
}
