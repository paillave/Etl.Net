using Paillave.Etl.Core.Aggregation.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.Aggregation;

public class AggregationProcessor<TRow, TValue, TOut, TAggregator> where TAggregator : AggregatorBase<TRow, TValue>
{
    private readonly List<TAggregator> _emptyAggregations;

    public AggregationProcessor(Expression<Func<TValue, TOut>> aggregationDescriptor)
    {
        AggregationDescriptorVisitor<TRow, TValue, TAggregator> vis = new();
        vis.Visit(aggregationDescriptor);
        if(vis.AggregationsToProcess==null)
            throw new InvalidOperationException("No aggregation found in the descriptor");
        this._emptyAggregations = vis.AggregationsToProcess;
    }
    public TOut CreateInstance(Dictionary<string, TAggregator> aggregators) => ObjectBuilder<TOut>.CreateInstance(aggregators.ToDictionary(i => i.Key, i => i.Value.GetResult()));
    public Dictionary<string, TAggregator?> CreateAggregators(TRow firstGroupValue)
    {
        return _emptyAggregations.ToDictionary(i => i.Name, i => i.CopyEmpty() as TAggregator);
    }
    public Dictionary<string, TAggregator> Aggregate(Dictionary<string, TAggregator> aggregators, TRow input)
    {
        foreach (var item in aggregators)
            item.Value.Aggregate(input);
        return aggregators;
    }
}
