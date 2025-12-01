using System;
using Paillave.Etl.Core;
using System.Text;
using System.Collections.Generic;
using System.Linq.Expressions;
using Paillave.Etl.Core.Mapping;
using System.Text.Json.Nodes;

namespace Paillave.Etl.TextFile
{
    public class FlatFileArgBuilder
    {
        public FlatFileDefinition<T> UseMap<T>(Expression<Func<IFieldMapper, T>> expression) => FlatFileDefinition.Create(expression);
        public FlatFileDefinition<T> UseType<T>() => new();
        public FlatFileDefinition<T> UseType<T>(T prototype) => new();
    }

    public static class TextFileEx
    {
        #region CrossApplyTextFile
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<IFileValue> stream, string name, Func<FlatFileArgBuilder, FlatFileDefinition<TOut>> mapBuilder, bool noParallelisation = false, bool useStreamCopy = true)
        {
            var valuesProvider = new FlatFileValuesProvider<TOut, TOut>(new FlatFileValuesProviderArgs<TOut, TOut>()
            {
                Mapping = mapBuilder(new()),
                ResultSelector = (i, o) => o,
                UseStreamCopy = useStreamCopy
            });
            return stream.CrossApply<IFileValue, TOut>(name, valuesProvider, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<IFileValue> stream, string name, FlatFileDefinition<TOut> args, bool noParallelisation = false, bool useStreamCopy = true)
        {
            var valuesProvider = new FlatFileValuesProvider<TOut, TOut>(new FlatFileValuesProviderArgs<TOut, TOut>()
            {
                Mapping = args,
                ResultSelector = (i, o) => o,
                UseStreamCopy = useStreamCopy
            });
            return stream.CrossApply<IFileValue, TOut>(name, valuesProvider, noParallelisation);
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<IFileValue> stream, string name, FlatFileDefinition<TParsed> args, Func<IFileValue, TParsed, TOut> resultSelector, bool noParallelisation = false, bool useStreamCopy = true)
        {
            var valuesProvider = new FlatFileValuesProvider<TParsed, TOut>(new FlatFileValuesProviderArgs<TParsed, TOut>()
            {
                Mapping = args,
                ResultSelector = resultSelector,
                UseStreamCopy = useStreamCopy
            });
            return stream.CrossApply<IFileValue, TOut>(name, valuesProvider, noParallelisation);
        }
        #endregion

        #region ToTextFile
        public static ISingleStream<IFileValue> ToTextFileValue<TIn>(this IStream<TIn> stream, string name, string fileName, FlatFileDefinition<TIn> mapping, Dictionary<string, IEnumerable<Destination>> destinations = null, JsonNode? extraMetadata = null, Encoding encoding = null)
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
        public static ISingleStream<IFileValue> ToTextFileValue<TIn>(this IStream<Correlated<TIn>> stream, string name, string fileName, FlatFileDefinition<TIn> mapping, Dictionary<string, IEnumerable<Destination>> destinations = null, JsonNode? extraMetadata = null, Encoding encoding = null)
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
        public static ISingleStream<IFileValue> ToTextFileValue<TIn>(this IStream<TIn> stream, string name, string fileName, Func<FlatFileDefinition<TIn>, FlatFileDefinition<TIn>> mapBuilder, Dictionary<string, IEnumerable<Destination>> destinations = null, JsonNode? extraMetadata = null, Encoding encoding = null)
        {
            return new ToFileValueStreamNode<TIn, TIn>(name, new ToTextDataStreamArgs<TIn, TIn>
            {
                MainStream = stream,
                Mapping = mapBuilder(new()),
                GetRow = i => i,
                FileName = fileName,
                Encoding = encoding,
                Metadata = extraMetadata,
                Destinations = destinations
            }).Output;
        }
        public static ISingleStream<IFileValue> ToTextFileValue<TIn>(this IStream<Correlated<TIn>> stream, string name, string fileName, Func<FlatFileDefinition<TIn>, FlatFileDefinition<TIn>> mapBuilder, Dictionary<string, IEnumerable<Destination>> destinations = null, JsonNode? extraMetadata = null, Encoding encoding = null)
        {
            return new ToFileValueStreamNode<Correlated<TIn>, TIn>(name, new ToTextDataStreamArgs<Correlated<TIn>, TIn>
            {
                MainStream = stream,
                Mapping = mapBuilder(new()),
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
