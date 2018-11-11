using Paillave.Etl.Core.Streams;
using Paillave.Etl.TextFile.ValuesProviders;
using Paillave.Etl.TextFile.Core;
using System;
using System.IO;
using System.Linq;
using Paillave.Etl.TextFile.StreamNodes;
using SystemIO = System.IO;
using Paillave.Etl;
using Paillave.Etl.Extensions;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.TextFile.Extensions
{
    public static class TextFileEx
    {
        #region CrossApplyTextFile
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<string> stream, string name, FlatFileDefinition<TOut> args, bool noParallelisation = false)
        {
            var valuesProvider = new FlatFileValuesProvider<string, TOut, TOut>(new FlatFileValuesProviderArgs<string, TOut, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                Mapping = args,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<string, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<LocalFilesValue> stream, string name, FlatFileDefinition<TOut> args, bool noParallelisation = false)
        {
            var valuesProvider = new FlatFileValuesProvider<LocalFilesValue, TOut, TOut>(new FlatFileValuesProviderArgs<LocalFilesValue, TOut, TOut>()
            {
                DataStreamSelector = i => i.GetContent(),
                Mapping = args,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<LocalFilesValue, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<Stream> stream, string name, FlatFileDefinition<TOut> args, bool noParallelisation = false)
        {
            var valuesProvider = new FlatFileValuesProvider<Stream, TOut, TOut>(new FlatFileValuesProviderArgs<Stream, TOut, TOut>()
            {
                DataStreamSelector = i => i,
                Mapping = args,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<Stream, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TOut>(this IStream<TIn> stream, string name, FlatFileDefinition<TOut> args, Func<TIn, string> filePathSelector, bool noParallelisation = false)
        {
            var valuesProvider = new FlatFileValuesProvider<TIn, TOut, TOut>(new FlatFileValuesProviderArgs<TIn, TOut, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                Mapping = args,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<TIn, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, FlatFileDefinition<TParsed> args, Func<TIn, string> filePathSelector, Func<TIn, TParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new FlatFileValuesProvider<TIn, TParsed, TOut>(new FlatFileValuesProviderArgs<TIn, TParsed, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                Mapping = args,
                ResultSelector = resultSelector
            });
            return stream.CrossApply<TIn, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<string> stream, string name, FlatFileDefinition<TParsed> args, Func<string, TParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new FlatFileValuesProvider<string, TParsed, TOut>(new FlatFileValuesProviderArgs<string, TParsed, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                Mapping = args,
                ResultSelector = resultSelector
            });
            return stream.CrossApply<string, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<Stream> stream, string name, FlatFileDefinition<TParsed> args, Func<TParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new FlatFileValuesProvider<Stream, TParsed, TOut>(new FlatFileValuesProviderArgs<Stream, TParsed, TOut>()
            {
                DataStreamSelector = i => i,
                Mapping = args,
                ResultSelector = (s, o) => resultSelector(o)
            });
            return stream.CrossApply<Stream, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }

        public static IStream<string> CrossApplyTextFile(this IStream<string> stream, string name, bool noParallelisation = false)
        {
            var valuesProvider = new TextFileValuesProvider();
            return stream.CrossApply<string, Stream, string, string>(name, valuesProvider.PushValues, i => File.OpenRead(i), (i, _) => i, noParallelisation);
        }
        public static IStream<string> CrossApplyTextFile(this IStream<Stream> stream, string name, bool noParallelisation = false)
        {
            var valuesProvider = new TextFileValuesProvider();
            return stream.CrossApply<Stream, string>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<string> CrossApplyTextFile<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> filePathSelector, bool noParallelisation = false)
        {
            var valuesProvider = new TextFileValuesProvider();
            return stream.CrossApply<TIn, Stream, string, string>(name, valuesProvider.PushValues, i => File.OpenRead(filePathSelector(i)), (i, _) => i, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> filePathSelector, Func<string, TIn, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new TextFileValuesProvider();
            return stream.CrossApply<TIn, Stream, string, TOut>(name, valuesProvider.PushValues, i => File.OpenRead(filePathSelector(i)), resultSelector, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<string> stream, string name, Func<string, string, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new TextFileValuesProvider();
            return stream.CrossApply<string, Stream, string, TOut>(name, valuesProvider.PushValues, i => File.OpenRead(i), resultSelector, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<Stream> stream, string name, Func<string, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new TextFileValuesProvider();
            return stream.CrossApply<Stream, Stream, string, TOut>(name, valuesProvider.PushValues, i => i, (i, _) => resultSelector(i), noParallelisation);
        }
        #endregion

        #region ThroughTextFile
        public static IStream<TIn> ThroughTextFile<TIn>(this IStream<TIn> stream, string name, ISingleStream<SystemIO.Stream> resourceStream, FlatFileDefinition<TIn> mapping)
        {
            return new ThroughFlatFileStreamNode<TIn, IStream<TIn>>(name, new ThroughFlatFileArgs<TIn, IStream<TIn>>
            {
                MainStream = stream,
                Mapping = mapping,
                TargetStream = resourceStream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughTextFile<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, ISingleStream<SystemIO.Stream> resourceStream, FlatFileDefinition<TIn> mapping)
        {
            return new ThroughFlatFileStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ThroughFlatFileArgs<TIn, ISortedStream<TIn, TKey>>
            {
                MainStream = stream,
                Mapping = mapping,
                TargetStream = resourceStream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughTextFile<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, ISingleStream<SystemIO.Stream> resourceStream, FlatFileDefinition<TIn> mapping)
        {
            return new ThroughFlatFileStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ThroughFlatFileArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                MainStream = stream,
                Mapping = mapping,
                TargetStream = resourceStream
            }).Output;
        }
        #endregion

        #region ToTextFile
        public static IStream<Stream> ToTextFile<TIn>(this IStream<TIn> stream, string name, FlatFileDefinition<TIn> mapping)
        {
            return new ToFlatFileStreamNode<TIn>(name, new ToFlatFileArgs<TIn>
            {
                MainStream = stream,
                Mapping = mapping,
            }).Output;
        }
        #endregion
    }
}
