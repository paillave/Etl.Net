using Paillave.Etl.Core.Aggregation.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.Aggregation
{
    public class AggregationProcessor<TIn, TOut>
    {
        private readonly List<Aggregator<TIn>> _emptyAggregations;

        public AggregationProcessor(Expression<Func<TIn, TOut>> aggregationDescriptor)
        {
            AggregationDescriptorVisitor<TIn> vis = new AggregationDescriptorVisitor<TIn>();
            vis.Visit(aggregationDescriptor);
            this._emptyAggregations = vis.AggregationsToProcess;
        }
        public TOut CreateInstance(Dictionary<string, Aggregator<TIn>> aggregators) => ObjectBuilder<TOut>.CreateInstance(aggregators.ToDictionary(i => i.Key, i => i.Value.GetResult()));
        public Dictionary<string, Aggregator<TIn>> CreateAggregators(TIn firstGroupValue)
        {
            return _emptyAggregations.ToDictionary(i => i.Name, i => i.CopyEmpty());
        }
        public Dictionary<string, Aggregator<TIn>> Aggregate(Dictionary<string, Aggregator<TIn>> aggregators, TIn input)
        {
            foreach (var item in aggregators)
                item.Value.Aggregate(input);
            return aggregators;
        }
    }
}
