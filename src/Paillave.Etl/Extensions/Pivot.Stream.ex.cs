using System;
using System.Linq.Expressions;

namespace Paillave.Etl.Core
{
    public static partial class PivotEx
    {
        public static IStream<AggregationResult<TIn, TKey, TAggr>> Pivot<TIn, TAggr, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, Expression<Func<TIn, TAggr>> aggregationDescriptor) =>
            new PivotStreamNode<TIn, TAggr, TKey>(name, new PivotArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                AggregationDescriptor = aggregationDescriptor,
                GetKey = getKey,
            }).Output;
        public static ISortedStream<AggregationResult<TIn, TKey, TAggr>, TKey> Pivot<TIn, TAggr, TKey>(this ISortedStream<TIn, TKey> stream, string name, Expression<Func<TIn, TAggr>> aggregationDescriptor) =>
            new PivotSortedStreamNode<TIn, TAggr, TKey>(name, new PivotSortedArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                AggregationDescriptor = aggregationDescriptor
            }).Output;
        public static IStream<Correlated<AggregationResult<TIn, TKey, TAggr>>> Pivot<TIn, TAggr, TKey>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> getKey, Expression<Func<TIn, TAggr>> aggregationDescriptor) =>
            new PivotCorrelatedStreamNode<TIn, TAggr, TKey>(name, new PivotCorrelatedArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                AggregationDescriptor = aggregationDescriptor,
                GetKey = getKey,
            }).Output;
        public static ISortedStream<Correlated<AggregationResult<TIn, TKey, TAggr>>, TKey> Pivot<TIn, TAggr, TKey>(this ISortedStream<Correlated<TIn>, TKey> stream, string name, Expression<Func<TIn, TAggr>> aggregationDescriptor) =>
            new PivotSortedCorrelatedStreamNode<TIn, TAggr, TKey>(name, new PivotSortedCorrelatedArgs<TIn, TAggr, TKey>
            {
                InputStream = stream,
                AggregationDescriptor = aggregationDescriptor
            }).Output;
    }
}
