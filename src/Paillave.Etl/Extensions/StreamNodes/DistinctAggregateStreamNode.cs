using Paillave.Etl.Core.Aggregation.AggregationInstances;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.Core;

public class AggregationBuilder<T>
{
    private readonly Dictionary<string, DistinctAggregator> _aggregators = new();
    public AggregationBuilder<T> ForProperty<X>(Expression<Func<T, X>> getProperty, DistinctAggregator aggregator)
    {
        _aggregators[getProperty.GetPropertyInfo().Name] = aggregator;
        return this;
    }
    internal ObjectAggregator<T> CreateObjectAggregator()
        => new(CreateAggregators());
    internal CorrelatedObjectAggregator<T> CreateCorrelatedObjectAggregator()
        => new(CreateAggregators());
    private Dictionary<string, IAggregationInstance> CreateAggregators()
        => typeof(T).GetProperties().GroupJoin(_aggregators, l => l.Name, r => r.Key, (l, r) => new
        {
            l.Name,
            AggregatorInstance = CreateAggregatorInstance(r.Select(i => i.Value).DefaultIfEmpty(DistinctAggregator.First).First())
        })
        .ToDictionary(i => i.Name, i => i.AggregatorInstance);
    private IAggregationInstance CreateAggregatorInstance(DistinctAggregator aggregator)
    {
        switch (aggregator)
        {
            case DistinctAggregator.First: return new FirstAggregationInstance();
            case DistinctAggregator.FirstNotNull: return new FirstNotNullAggregationInstance();
            case DistinctAggregator.Last: return new LastAggregationInstance();
            case DistinctAggregator.Max: return new MaxAggregationInstance();
            case DistinctAggregator.Min: return new MinAggregationInstance();
            case DistinctAggregator.Sum: return new SumAggregationInstance();
            case DistinctAggregator.Avg: return new AvgAggregationInstance();
        }
        return null;
    }
}
internal class ObjectAggregator<T>(Dictionary<string, IAggregationInstance> aggregations)
{
    private readonly Dictionary<string, IAggregationInstance> _aggregations = aggregations;

    public ObjectAggregator<T> Aggregate(T value)
    {
        if (value == null) return this;
        foreach (var property in typeof(T).GetProperties())
            _aggregations[property.Name].Aggregate(property.GetValue(value));
        return this;
    }
    public T GetAggregation() => ObjectBuilder<T>.CreateInstance(_aggregations.ToDictionary(i => i.Key, i => i.Value.GetResult()));
}
internal class CorrelatedObjectAggregator<T>(Dictionary<string, IAggregationInstance> aggregations)
{
    private readonly Dictionary<string, IAggregationInstance> _aggregations = aggregations;
    private readonly HashSet<Guid> _correlationKeys = new();

    public CorrelatedObjectAggregator<T> Aggregate(Correlated<T> value)
    {
        _correlationKeys.UnionWith(value.CorrelationKeys);
        if (value.Row == null) return this;
        foreach (var property in typeof(T).GetProperties())
            _aggregations[property.Name].Aggregate(property.GetValue(value.Row));
        return this;
    }
    public Correlated<T> GetAggregation() => new()
    {
        Row = ObjectBuilder<T>.CreateInstance(_aggregations.ToDictionary(i => i.Key, i => i.Value.GetResult())),
        CorrelationKeys = _correlationKeys
    };
}
public class DistinctAggregateArgs<TIn, TGroupingKey>
{
    public IStream<TIn> InputStream { get; set; }
    public Func<TIn, TGroupingKey> GetGroupingKey { get; set; }
    public AggregationBuilder<TIn> Aggregator { get; set; }
}
public class DistinctAggregateStreamNode<TIn, TGroupingKey>(string name, DistinctAggregateArgs<TIn, TGroupingKey> args) : StreamNodeBase<TIn, IStream<TIn>, DistinctAggregateArgs<TIn, TGroupingKey>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override IStream<TIn> CreateOutputStream(DistinctAggregateArgs<TIn, TGroupingKey> args)
        => base.CreateUnsortedStream(args.InputStream.Observable.Aggregate(
            i => args.Aggregator.CreateObjectAggregator(),
            args.GetGroupingKey,
            (aggr, input) => aggr.Aggregate(input),
            (input, key, aggr) => aggr.GetAggregation()));
}
public class DistinctAggregateCorrelatedArgs<TIn, TGroupingKey>
{
    public IStream<Correlated<TIn>> InputStream { get; set; }
    public Func<TIn, TGroupingKey> GetGroupingKey { get; set; }
    public AggregationBuilder<TIn> Aggregator { get; set; }
}
public class DistinctAggregateCorrelatedStreamNode<TIn, TGroupingKey>(string name, DistinctAggregateCorrelatedArgs<TIn, TGroupingKey> args) : StreamNodeBase<Correlated<TIn>, IStream<Correlated<TIn>>, DistinctAggregateCorrelatedArgs<TIn, TGroupingKey>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    protected override IStream<Correlated<TIn>> CreateOutputStream(DistinctAggregateCorrelatedArgs<TIn, TGroupingKey> args)
        => base.CreateUnsortedStream(args.InputStream.Observable.Aggregate(
            i => args.Aggregator.CreateCorrelatedObjectAggregator(),
            i => args.GetGroupingKey(i.Row),
            (aggr, input) => aggr.Aggregate(input),
            (input, key, aggr) => aggr.GetAggregation()));
}
