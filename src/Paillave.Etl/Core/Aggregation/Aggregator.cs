using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation
{
    public class Aggregator<TIn>
    {
        public void SetFilter(PropertyInfo filteredPropertyInfo, object value)
        {
            this._filteredPropertyInfo = filteredPropertyInfo;
            this._filterValue = value;
        }
        private readonly PropertyInfo _sourcePropertyInfo;
        private IAggregationInstance _aggregationInstance = null;
        private PropertyInfo _filteredPropertyInfo;
        private object _filterValue;
        private readonly Type _aggregatorInstanceType;
        public PropertyInfo TargetPropertyInfo { get; }
        public string Name { get; }
        public Aggregator(Type aggregatorInstanceType, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo)
        {
            _aggregatorInstanceType = aggregatorInstanceType;
            _sourcePropertyInfo = sourcePropertyInfo;
            TargetPropertyInfo = targetPropertyInfo;
            if (aggregatorInstanceType != null)
                _aggregationInstance = (IAggregationInstance)Activator.CreateInstance(aggregatorInstanceType);
            Name = targetPropertyInfo.Name;
        }
        public Aggregator<TIn> CopyEmpty()
        {
            var agg = new Aggregator<TIn>(_aggregatorInstanceType, _sourcePropertyInfo, TargetPropertyInfo);
            if (_filteredPropertyInfo != null)
                agg.SetFilter(_filteredPropertyInfo, _filterValue);
            return agg;
        }
        public void Aggregate(TIn input)
        {
            if (CanAggregate(input))
                _aggregationInstance?.Aggregate(_sourcePropertyInfo?.GetValue(input));
        }
        private bool CanAggregate(TIn input)
        {
            if (_filteredPropertyInfo == null) return true;
            var value = _filteredPropertyInfo.GetValue(input);
            if (value == null && _filterValue == null) return true;
            if (value == null && _filterValue != null) return false;
            if (value != null && _filterValue == null) return false;
            return value.Equals(_filterValue);
        }
        public object GetResult() => _aggregationInstance?.GetResult();
    }
}
