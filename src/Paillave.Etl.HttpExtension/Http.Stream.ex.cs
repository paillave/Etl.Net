// using System;
// using System.Collections.Generic;
// using System.Linq.Expressions;
// using System.Text;
// using Paillave.Etl.Core;
// using Paillave.Etl.Core.Mapping;


// namespace Paillave.Etl.HttpExtension;
// {
//     public class HttpArgBuilder
//     {
//         public HttpDefinition<T> UseMap<T>(Expression<Func<IFieldMapper, T>> expression) =>
//             HttpDefinition.Create(expression);

//         public HttpDefinition<T> UseType<T>() => new HttpDefinition<T>();

//         public HttpDefinition<T> UseType<T>(T prototype) => new HttpDefinition<T>();
//     }

//     public static class TextFileEx
//     {
//         #region CrossApplyTextFile
//         public static IStream<TOut> CrossApplyTextFile<TOut>(
//             this IStream<IFileValue> stream,
//             string name,
//             Func<HttpArgBuilder, HttpDefinition<TOut>> mapBuilder,
//             bool noParallelisation = false,
//             bool useStreamCopy = true
//         )
//         {
//             var valuesProvider = new HttpValuesProvider<TOut, TOut>(
//                 new HttpValuesProviderArgs<TOut, TOut>()
//                 {
//                     Mapping = mapBuilder(new()),
//                     ResultSelector = (i, o) => o,
//                     UseStreamCopy = useStreamCopy,
//                 }
//             );
//             return stream.CrossApply<IFileValue, TOut>(name, valuesProvider, noParallelisation);
//         }

//         public static IStream<TOut> CrossApplyTextFile<TOut>(
//             this IStream<IFileValue> stream,
//             string name,
//             HttpDefinition<TOut> args,
//             bool noParallelisation = false,
//             bool useStreamCopy = true
//         )
//         {
//             var valuesProvider = new HttpValuesProvider<TOut, TOut>(
//                 new HttpValuesProviderArgs<TOut, TOut>()
//                 {
//                     Mapping = args,
//                     ResultSelector = (i, o) => o,
//                     UseStreamCopy = useStreamCopy,
//                 }
//             );
//             return stream.CrossApply<IFileValue, TOut>(name, valuesProvider, noParallelisation);
//         }

//         public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(
//             this IStream<IFileValue> stream,
//             string name,
//             HttpDefinition<TParsed> args,
//             Func<IFileValue, TParsed, TOut> resultSelector,
//             bool noParallelisation = false,
//             bool useStreamCopy = true
//         )
//         {
//             var valuesProvider = new HttpValuesProvider<TParsed, TOut>(
//                 new HttpValuesProviderArgs<TParsed, TOut>()
//                 {
//                     Mapping = args,
//                     ResultSelector = resultSelector,
//                     UseStreamCopy = useStreamCopy,
//                 }
//             );
//             return stream.CrossApply<IFileValue, TOut>(name, valuesProvider, noParallelisation);
//         }
//         #endregion

//         #region ToTextFile
//         public static ISingleStream<IFileValue> ToTextFileValue<TIn>(
//             this IStream<TIn> stream,
//             string name,
//             string fileName,
//             HttpDefinition<TIn> mapping,
//             Dictionary<string, IEnumerable<Destination>> destinations = null,
//             object extraMetadata = null,
//             Encoding encoding = null
//         )
//         {
//             return new ToFileValueStreamNode<TIn, TIn>(
//                 name,
//                 new ToTextDataStreamArgs<TIn, TIn>
//                 {
//                     MainStream = stream,
//                     Mapping = mapping,
//                     GetRow = i => i,
//                     FileName = fileName,
//                     Encoding = encoding,
//                     Metadata = extraMetadata,
//                     Destinations = destinations,
//                 }
//             ).Output;
//         }

//         public static ISingleStream<IFileValue> ToTextFileValue<TIn>(
//             this IStream<Correlated<TIn>> stream,
//             string name,
//             string fileName,
//             HttpDefinition<TIn> mapping,
//             Dictionary<string, IEnumerable<Destination>> destinations = null,
//             object extraMetadata = null,
//             Encoding encoding = null
//         )
//         {
//             return new ToFileValueStreamNode<Correlated<TIn>, TIn>(
//                 name,
//                 new ToTextDataStreamArgs<Correlated<TIn>, TIn>
//                 {
//                     MainStream = stream,
//                     Mapping = mapping,
//                     GetRow = i => i.Row,
//                     FileName = fileName,
//                     Encoding = encoding,
//                     Metadata = extraMetadata,
//                     Destinations = destinations,
//                 }
//             ).Output;
//         }

//         public static ISingleStream<IFileValue> ToTextFileValue<TIn>(
//             this IStream<TIn> stream,
//             string name,
//             string fileName,
//             Func<HttpDefinition<TIn>, HttpDefinition<TIn>> mapBuilder,
//             Dictionary<string, IEnumerable<Destination>> destinations = null,
//             object extraMetadata = null,
//             Encoding encoding = null
//         )
//         {
//             return new ToFileValueStreamNode<TIn, TIn>(
//                 name,
//                 new ToTextDataStreamArgs<TIn, TIn>
//                 {
//                     MainStream = stream,
//                     Mapping = mapBuilder(new()),
//                     GetRow = i => i,
//                     FileName = fileName,
//                     Encoding = encoding,
//                     Metadata = extraMetadata,
//                     Destinations = destinations,
//                 }
//             ).Output;
//         }

//         public static ISingleStream<IFileValue> ToTextFileValue<TIn>(
//             this IStream<Correlated<TIn>> stream,
//             string name,
//             string fileName,
//             Func<HttpDefinition<TIn>, HttpDefinition<TIn>> mapBuilder,
//             Dictionary<string, IEnumerable<Destination>> destinations = null,
//             object extraMetadata = null,
//             Encoding encoding = null
//         )
//         {
//             return new ToFileValueStreamNode<Correlated<TIn>, TIn>(
//                 name,
//                 new ToTextDataStreamArgs<Correlated<TIn>, TIn>
//                 {
//                     MainStream = stream,
//                     Mapping = mapBuilder(new()),
//                     GetRow = i => i.Row,
//                     FileName = fileName,
//                     Encoding = encoding,
//                     Metadata = extraMetadata,
//                     Destinations = destinations,
//                 }
//             ).Output;
//         }
//         #endregion
//     }
// }
