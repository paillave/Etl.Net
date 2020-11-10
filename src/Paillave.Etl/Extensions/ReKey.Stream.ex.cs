using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;
using System.Linq.Expressions;

namespace Paillave.Etl.Extensions
{
    public static partial class ReKeyEx
    {
        public static IStream<TOut> ReKey<TIn, TKeys, TOut>(this IStream<TIn> stream, string name, Func<TIn, TKeys> getKeys, Func<TIn, TKeys, TOut> resultSelector)
        {
            return new ReKeyStreamNode<TIn, TOut, TKeys>(name, new ReKeyArgs<TIn, TOut, TKeys>
            {
                InputStream = stream,
                ResultSelector = resultSelector,
                GetKeys = getKeys
            }).Output;
        }
        public static IStream<Correlated<TOut>> ReKey<TIn, TKeys, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKeys> getKeys, Func<TIn, TKeys, TOut> resultSelector)
        {
            return new ReKeyStreamNode<Correlated<TIn>, Correlated<TOut>, TKeys>(name, new ReKeyArgs<Correlated<TIn>, Correlated<TOut>, TKeys>
            {
                InputStream = stream,
                ResultSelector = (i, k) => new Correlated<TOut> { Row = resultSelector(i.Row, k), CorrelationKeys = i.CorrelationKeys },
                GetKeys = i => getKeys(i.Row)
            }).Output;
        }

        public static IStream<TIn> ReKey<TIn>(this IStream<TIn> stream, string name, Expression<Func<TIn, object>> getKeys)
        {
            return new ReKey2StreamNode<TIn, TIn, TIn>(name, new ReKey2Args<TIn, TIn, TIn>
            {
                InputStream = stream,
                ResultSelector = (i, j) => j,
                RowSelector = i => i,
                GetKeys = getKeys
            }).Output;
        }
        public static IStream<Correlated<TIn>> ReKey<TIn>(this IStream<Correlated<TIn>> stream, string name, Expression<Func<TIn, object>> getKeys)
        {
            return new ReKey2StreamNode<Correlated<TIn>, TIn, Correlated<TIn>>(name, new ReKey2Args<Correlated<TIn>, TIn, Correlated<TIn>>
            {
                InputStream = stream,
                ResultSelector = (i, k) => new Correlated<TIn> { Row = k, CorrelationKeys = i.CorrelationKeys },
                GetKeys = getKeys,
                RowSelector = i => i.Row
            }).Output;
        }
    }
}
