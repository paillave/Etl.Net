using System;
using SystemIO = System.IO;
using Paillave.Etl.Core;
using System.Text;
using System.Collections.Generic;

namespace Paillave.Etl.TextFile
{
    public static class TextFileEx
    {
        #region CrossApplyTextFile
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<IFileValue> stream, string name, FlatFileDefinition<TOut> args, bool noParallelisation = false)
        {
            var valuesProvider = new FlatFileValuesProvider<TOut, TOut>(new FlatFileValuesProviderArgs<TOut, TOut>()
            {
                Mapping = args,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<IFileValue, TOut>(name, valuesProvider, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<IFileValue> stream, string name, FlatFileDefinition<TParsed> args, Func<IFileValue, TParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new FlatFileValuesProvider<TParsed, TOut>(new FlatFileValuesProviderArgs<TParsed, TOut>()
            {
                Mapping = args,
                ResultSelector = resultSelector
            });
            return stream.CrossApply<IFileValue, TOut>(name, valuesProvider, noParallelisation);
        }
        #endregion

        #region ThroughTextFile
        public static IStream<TIn> ToTextDataStream<TIn>(this IStream<TIn> stream, string name, ISingleStream<SystemIO.Stream> resourceStream, FlatFileDefinition<TIn> mapping)
        {
            return new ToTextDataStreamStreamNode<TIn, IStream<TIn>>(name, new ToTextDataStreamFileArgs<TIn, IStream<TIn>>
            {
                MainStream = stream,
                Mapping = mapping,
                TargetDataStream = resourceStream
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToTextDataStream<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, ISingleStream<SystemIO.Stream> resourceStream, FlatFileDefinition<TIn> mapping)
        {
            return new ToTextDataStreamStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ToTextDataStreamFileArgs<TIn, ISortedStream<TIn, TKey>>
            {
                MainStream = stream,
                Mapping = mapping,
                TargetDataStream = resourceStream
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToTextDataStream<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, ISingleStream<SystemIO.Stream> resourceStream, FlatFileDefinition<TIn> mapping)
        {
            return new ToTextDataStreamStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ToTextDataStreamFileArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                MainStream = stream,
                Mapping = mapping,
                TargetDataStream = resourceStream
            }).Output;
        }
        #endregion

        #region ToTextFile
        public static ISingleStream<IFileValue> ToTextFileValue<TIn>(this IStream<TIn> stream, string name, string fileName, FlatFileDefinition<TIn> mapping, Dictionary<string, List<Destination>> destinations = null, object extraMetadata = null, Encoding encoding = null)
        {
            return new ToFileValueStreamNode<TIn, TIn>(name, new ToTextDataStreamArgs<TIn, TIn>
            {
                MainStream = stream,
                Mapping = mapping,
                GetRow = i => i,
                FileName = fileName,
                Encoding = encoding,
                Metadata = extraMetadata,
                Destinations = destinations
            }).Output;
        }
        public static ISingleStream<IFileValue> ToTextFileValue<TIn>(this IStream<Correlated<TIn>> stream, string name, string fileName, FlatFileDefinition<TIn> mapping, Dictionary<string, List<Destination>> destinations = null, object extraMetadata = null, Encoding encoding = null)
        {
            return new ToFileValueStreamNode<Correlated<TIn>, TIn>(name, new ToTextDataStreamArgs<Correlated<TIn>, TIn>
            {
                MainStream = stream,
                Mapping = mapping,
                GetRow = i => i.Row,
                FileName = fileName,
                Encoding = encoding,
                Metadata = extraMetadata,
                Destinations = destinations
            }).Output;
        }
        #endregion
    }
}
