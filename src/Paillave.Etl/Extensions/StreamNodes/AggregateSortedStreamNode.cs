using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Core;

public class AggregateSortedArgs<TIn, TAggrRes, TKey>
{
    public ISortedStream<TIn, TKey> InputStream { get; set; }
    public Func<TAggrRes, TIn, TAggrRes> Aggregate { get; set; }
    public Func<TIn, TAggrRes> CreateEmptyAggregation { get; set; }
}
public class AggregateSortedStreamNode<TIn, TAggrRes, TKey>(string name, AggregateSortedArgs<TIn, TAggrRes, TKey> args) : StreamNodeBase<AggregationResult<TIn, TKey, TAggrRes>, ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey>, AggregateSortedArgs<TIn, TAggrRes, TKey>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey> CreateOutputStream(AggregateSortedArgs<TIn, TAggrRes, TKey> args)
    {
        return CreateSortedStream(
            args.InputStream.Observable.AggregateGrouped(
                args.CreateEmptyAggregation,
                args.InputStream.SortDefinition,
                args.Aggregate,
                (i, a) => new AggregationResult<TIn, TKey, TAggrRes>
                {
                    FirstValue = i,
                    Aggregation = a,
                    Key = args.InputStream.SortDefinition.GetKey(i)
                }),
            new SortDefinition<AggregationResult<TIn, TKey, TAggrRes>, TKey>(i => i.Key, args.InputStream.SortDefinition.KeyPosition));
    }
}
public class AggregateCorrelatedSortedArgs<TIn, TAggrRes, TKey>
{
    public ISortedStream<Correlated<TIn>, TKey> InputStream { get; set; }
    public Func<TAggrRes, TIn, TAggrRes> Aggregate { get; set; }
    public Func<TIn, TAggrRes> CreateEmptyAggregation { get; set; }
}
public class AggregateCorrelatedSortedStreamNode<TIn, TAggrRes, TKey>(string name, AggregateCorrelatedSortedArgs<TIn, TAggrRes, TKey> args) : StreamNodeBase<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, ISortedStream<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, TKey>, AggregateCorrelatedSortedArgs<TIn, TAggrRes, TKey>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override ISortedStream<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, TKey> CreateOutputStream(AggregateCorrelatedSortedArgs<TIn, TAggrRes, TKey> args)
    {
        return CreateSortedStream(
            args.InputStream.Observable.AggregateGrouped(
                i => new Correlated<TAggrRes>
                {
                    Row = args.CreateEmptyAggregation(i.Row),
                    CorrelationKeys = new HashSet<Guid>()
                },
                args.InputStream.SortDefinition,
                (a, i) =>
                {
                    a.Row = args.Aggregate(a.Row, i.Row);
                    a.CorrelationKeys.UnionWith(i.CorrelationKeys);
                    return a;
                },
                (i, a) => new Correlated<AggregationResult<TIn, TKey, TAggrRes>>
                {
                    CorrelationKeys = a.CorrelationKeys,
                    Row = new AggregationResult<TIn, TKey, TAggrRes>
                    {
                        Aggregation = a.Row,
                        FirstValue = i.Row,
                        Key = args.InputStream.SortDefinition.GetKey(i)
                    }
                }),
            new SortDefinition<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, TKey>(i => i.Row.Key, args.InputStream.SortDefinition.KeyPosition));
    }
}
