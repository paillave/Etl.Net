using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;

namespace Paillave.Etl.Extensions
{
    public static partial class GroupByEx
    {
        public static IStream<TOut> GroupBy<TIn, TKey, TOut>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, Func<IStream<TIn>, IStream<TOut>> subProcess)
        {
            return new GroupByStreamNode<TIn, TKey, TOut>(name, new GroupByArgs<TIn, TKey, TOut>
            {
                SubProcess = subProcess,
                Stream = stream,
                GetKey = getKey
            }).Output;
        }
        public static IStream<TOut> GroupBy<TIn, TKey, TOut>(this ISortedStream<TIn, TKey> stream, string name, Func<IStream<TIn>, IStream<TOut>> subProcess)
        {
            return new GroupBySortedStreamNode<TIn, TKey, TOut>(name, new GroupBySortedArgs<TIn, TKey, TOut>
            {
                SubProcess = subProcess,
                Stream = stream
            }).Output;
        }
        public static IStream<Correlated<TOut>> GroupBy<TIn, TKey, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> getKey, Func<IStream<Correlated<TIn>>, IStream<Correlated<TOut>>> subProcess)
        {
            return new GroupByCorrelatedStreamNode<TIn, TKey, TOut>(name, new GroupByCorrelatedArgs<TIn, TKey, TOut>
            {
                SubProcess = subProcess,
                Stream = stream,
                GetKey = getKey
            }).Output;
        }
        public static IStream<Correlated<TOut>> GroupBy<TIn, TKey, TOut>(this ISortedStream<Correlated<TIn>, TKey> stream, string name, Func<IStream<Correlated<TIn>>, IStream<Correlated<TOut>>> subProcess)
        {
            return new GroupByCorrelatedSortedStreamNode<TIn, TKey, TOut>(name, new GroupByCorrelatedSortedArgs<TIn, TKey, TOut>
            {
                SubProcess = subProcess,
                Stream = stream
            }).Output;
        }
    }
}
