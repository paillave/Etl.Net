using System;

namespace Paillave.Etl.Core;

public static partial class DistinctEx
{
    public static IStream<TIn> Distinct<TIn, TGroupingKey>(this IStream<TIn> stream, string name, Func<TIn, TGroupingKey> getKey, bool tryToExcludeNullValues = false)
    {
        if (tryToExcludeNullValues)
            return new SmartDistinctStreamNode<TIn, TGroupingKey>(name, new SmartDistinctArgs<TIn, TGroupingKey>
            {
                GetGroupingKey = getKey,
                InputStream = stream
            }).Output;
        else
            return new DistinctStreamNode<TIn, TGroupingKey>(name, new DistinctArgs<TIn, TGroupingKey>
            {
                GetGroupingKey = getKey,
                InputStream = stream
            }).Output;
    }
    public static IStream<TIn> Distinct<TIn, TGroupingKey>(this IStream<TIn> stream, string name, Func<TIn, TGroupingKey> getGroupingKey, Func<AggregationBuilder<TIn>, AggregationBuilder<TIn>> defineAggregations)
        => new DistinctAggregateStreamNode<TIn, TGroupingKey>(name, new DistinctAggregateArgs<TIn, TGroupingKey>
        {
            GetGroupingKey = getGroupingKey,
            InputStream = stream,
            Aggregator = defineAggregations(new AggregationBuilder<TIn>())
        }).Output;
    public static IStream<TIn> Distinct<TIn>(this IStream<TIn> stream, string name, Func<AggregationBuilder<TIn>, AggregationBuilder<TIn>> defineAggregations)
        => new DistinctAggregateStreamNode<TIn, bool>(name, new DistinctAggregateArgs<TIn, bool>
        {
            GetGroupingKey = i => true,
            InputStream = stream,
            Aggregator = defineAggregations(new AggregationBuilder<TIn>())
        }).Output;
    public static IKeyedStream<TIn, TGroupingKey> Distinct<TIn, TGroupingKey>(this ISortedStream<TIn, TGroupingKey> stream, string name, bool tryToExcludeNullValues = false)
    {
        if (tryToExcludeNullValues)
            return new SmartDistinctSortedStreamNode<TIn, TGroupingKey>(name, new SmartDistinctSortedArgs<TIn, TGroupingKey>
            {
                InputStream = stream
            }).Output;
        else
            return new DistinctSortedStreamNode<TIn, TGroupingKey>(name, new DistinctSortedArgs<TIn, TGroupingKey>
            {
                InputStream = stream
            }).Output;
    }
    public static IStream<Correlated<TIn>> Distinct<TIn, TGroupingKey>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TGroupingKey> getGroupingKey, Func<AggregationBuilder<TIn>, AggregationBuilder<TIn>> defineAggregations)
        => new DistinctAggregateCorrelatedStreamNode<TIn, TGroupingKey>(name, new DistinctAggregateCorrelatedArgs<TIn, TGroupingKey>
        {
            GetGroupingKey = getGroupingKey,
            InputStream = stream,
            Aggregator = defineAggregations(new AggregationBuilder<TIn>())
        }).Output;
    public static IStream<Correlated<TIn>> Distinct<TIn>(this IStream<Correlated<TIn>> stream, string name, Func<AggregationBuilder<TIn>, AggregationBuilder<TIn>> defineAggregations)
        => new DistinctAggregateCorrelatedStreamNode<TIn, bool>(name, new DistinctAggregateCorrelatedArgs<TIn, bool>
        {
            GetGroupingKey = i => true,
            InputStream = stream,
            Aggregator = defineAggregations(new AggregationBuilder<TIn>())
        }).Output;
    public static IStream<Correlated<TIn>> Distinct<TIn, TGroupingKey>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TGroupingKey> getGroupingKey, bool tryToExcludeNullValues = false)
    {
        if (tryToExcludeNullValues)
            return new SmartDistinctCorrelatedStreamNode<TIn, TGroupingKey>(name, new SmartDistinctCorrelatedArgs<TIn, TGroupingKey>
            {
                GetGroupingKey = getGroupingKey,
                InputStream = stream,
            }).Output;
        else
            return new DistinctCorrelatedStreamNode<TIn, TGroupingKey>(name, new DistinctCorrelatedArgs<TIn, TGroupingKey>
            {
                GetGroupingKey = getGroupingKey,
                InputStream = stream,
            }).Output;
    }
    public static IKeyedStream<Correlated<TIn>, TGroupingKey> Distinct<TIn, TGroupingKey>(this ISortedStream<Correlated<TIn>, TGroupingKey> stream, string name, bool tryToExcludeNullValues = false)
    {
        if (tryToExcludeNullValues)
            return new SmartDistinctCorrelatedSortedStreamNode<TIn, TGroupingKey>(name, new SmartDistinctCorrelatedSortedArgs<TIn, TGroupingKey>
            {
                InputStream = stream,
            }).Output;
        else
            return new DistinctCorrelatedSortedStreamNode<TIn, TGroupingKey>(name, new DistinctCorrelatedSortedArgs<TIn, TGroupingKey>
            {
                InputStream = stream,
            }).Output;
    }
}
