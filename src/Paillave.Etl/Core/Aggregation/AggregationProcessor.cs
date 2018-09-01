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
        private readonly ParameterInfo[] _anonymousConstructorParameters;
        private Type _outType = typeof(TOut);
        private readonly bool _isOutputAnonymous;


        public AggregationProcessor(Expression<Func<TIn, TOut>> aggregationDescriptor)
        {
            _isOutputAnonymous = Attribute.IsDefined(_outType, typeof(CompilerGeneratedAttribute), false);
            if (_isOutputAnonymous)
                this._anonymousConstructorParameters = _outType.GetConstructors()[0].GetParameters();
            AggregationDescriptorVisitor<TIn> vis = new AggregationDescriptorVisitor<TIn>();
            vis.Visit(aggregationDescriptor);
            this._emptyAggregations = vis.AggregationsToProcess;
        }
        public TOut CreateInstance(Dictionary<string, Aggregator<TIn>> aggregators)
        {
            TOut ret;
            if (_isOutputAnonymous)
            {
                ret = (TOut)Activator.CreateInstance(_outType, _anonymousConstructorParameters.Select(i => aggregators[i.Name].GetResult()).ToArray());
            }
            else
            {
                ret = Activator.CreateInstance<TOut>();
                foreach (var aggregator in aggregators)
                    aggregator.Value.TargetPropertyInfo.SetValue(ret, aggregator.Value.GetResult());
            }
            return ret;
        }
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
