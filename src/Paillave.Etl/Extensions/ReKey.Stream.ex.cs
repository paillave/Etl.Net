using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SystemIO = System.IO;

namespace Paillave.Etl.Extensions
{
    public static partial class ReKeyEx
    {
        /// <summary>
        /// Aggregate every element of a stream into a list of aggregations computed for each group by multiple keys
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="getKeys">Method to get the list of key of an element of the stream</param>
        /// <param name="resultSelector">Create the output row by using the full list of keys</param>
        /// <typeparam name="TIn">Main stream type</typeparam>
        /// <typeparam name="TAggr">Aggregation type</typeparam>
        /// <typeparam name="TKeys">Key type</typeparam>
        /// <returns>Output type</returns>
        public static IStream<TOut> ReKey<TIn, TKeys, TOut>(this IStream<TIn> stream, string name, Func<TIn, TKeys> getKeys, Func<TIn, TKeys, TOut> resultSelector)
        {
            return new ReKeyStreamNode<TIn, TOut, TKeys>(name, new ReKeyArgs<TIn, TOut, TKeys>
            {
                InputStream = stream,
                ResultSelector = resultSelector,
                GetKeys = getKeys
            }).Output;
        }
        /// <summary>
        /// Aggregate every element of a stream into a list of aggregations computed for each group by multiple keys
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="getKeys">Method to get the list of key of an element of the stream</param>
        /// <param name="resultSelector">Create the output row by using the full list of keys</param>
        /// <typeparam name="TIn">Main stream type</typeparam>
        /// <typeparam name="TAggr">Aggregation type</typeparam>
        /// <typeparam name="TKeys">Key type</typeparam>
        /// <returns>Output type</returns>
        public static IStream<Correlated<TOut>> ReKey<TIn, TKeys, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKeys> getKeys, Func<TIn, TKeys, TOut> resultSelector)
        {
            return new ReKeyStreamNode<Correlated<TIn>, Correlated<TOut>, TKeys>(name, new ReKeyArgs<Correlated<TIn>, Correlated<TOut>, TKeys>
            {
                InputStream = stream,
                ResultSelector = (i, k) => new Correlated<TOut> { Row = resultSelector(i.Row, k), CorrelationKeys = i.CorrelationKeys },
                GetKeys = i => getKeys(i.Row)
            }).Output;
        }
    }
}
