using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation
{
    public abstract class AggregatorBase<TIn, TValue>
    {
        public void SetFilter(PropertyInfo filteredPropertyInfo, object value)
        {
            this.FilteredPropertyInfo = filteredPropertyInfo;
            this.FilterValue = value;
        }
        protected PropertyInfo SourcePropertyInfo { get; }
        protected IAggregationInstance AggregationInstance { get; private set; } = null;
        protected PropertyInfo FilteredPropertyInfo { get; private set; }
        protected object FilterValue { get; private set; }
        protected Type AggregatorInstanceType { get; }
        public PropertyInfo TargetPropertyInfo { get; }
        protected abstract TValue GetInValue(TIn input);
        public string Name { get; }
        public AggregatorBase(Type aggregatorInstanceType, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo)
        {
            AggregatorInstanceType = aggregatorInstanceType;
            SourcePropertyInfo = sourcePropertyInfo;
            TargetPropertyInfo = targetPropertyInfo;
            if (aggregatorInstanceType != null)
                AggregationInstance = (IAggregationInstance)Activator.CreateInstance(aggregatorInstanceType);
            Name = targetPropertyInfo.Name;
        }
        public AggregatorBase<TIn, TValue> CopyEmpty()
        {
            var agg = CreateEmptyInstance(AggregatorInstanceType, SourcePropertyInfo, TargetPropertyInfo);
            if (FilteredPropertyInfo != null)
                agg.SetFilter(FilteredPropertyInfo, FilterValue);
            return agg;
        }
        protected abstract AggregatorBase<TIn, TValue> CreateEmptyInstance(Type aggregatorInstanceType, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo);
        public void Aggregate(TIn input)
        {
            if (CanAggregate(this.GetInValue(input)))
                this.InternalAggregate(input);
        }
        protected virtual void InternalAggregate(TIn input)
        {
            AggregationInstance?.Aggregate(SourcePropertyInfo?.GetValue(this.GetInValue(input)));
        }
        private bool CanAggregate(TValue input)
        {
            if (FilteredPropertyInfo == null) return true;
            var value = FilteredPropertyInfo.GetValue(input);
            if (value == null && FilterValue == null) return true;
            if (value == null && FilterValue != null) return false;
            if (value != null && FilterValue == null) return false;
            return value.Equals(FilterValue);
        }
        public object GetResult() => AggregationInstance?.GetResult();
    }
    public class Aggregator<TIn> : AggregatorBase<TIn, TIn>
    {
        public Aggregator(Type aggregatorInstanceType, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo)
            : base(aggregatorInstanceType, sourcePropertyInfo, targetPropertyInfo) { }
        protected override TIn GetInValue(TIn input) => input;
        protected override AggregatorBase<TIn, TIn> CreateEmptyInstance(Type aggregatorInstanceType, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo)
            => new Aggregator<TIn>(aggregatorInstanceType, sourcePropertyInfo, targetPropertyInfo);
    }
    public class CorrelatedAggregator<TCorrelated> : AggregatorBase<Correlated<TCorrelated>, TCorrelated>
    {
        public CorrelatedAggregator(Type aggregatorInstanceType, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo)
            : base(aggregatorInstanceType, sourcePropertyInfo, targetPropertyInfo) { }

        public HashSet<Guid> CorrelationKeys = new HashSet<Guid>();
        protected override AggregatorBase<Correlated<TCorrelated>, TCorrelated> CreateEmptyInstance(Type aggregatorInstanceType, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo)
            => new CorrelatedAggregator<TCorrelated>(aggregatorInstanceType, sourcePropertyInfo, targetPropertyInfo);
        protected override TCorrelated GetInValue(Correlated<TCorrelated> input) => input.Row;
        protected override void InternalAggregate(Correlated<TCorrelated> input)
        {
            base.InternalAggregate(input);
            this.CorrelationKeys.UnionWith(input.CorrelationKeys);
        }
    }
}
