using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation
{
    public class Aggregator<TIn>
    {
        private readonly PropertyInfo _sourcePropertyInfo;
        private IAggregationInstance _aggregationInstance;
        private readonly Type _aggregatorInstanceType;
        public PropertyInfo TargetPropertyInfo { get; }
        public string Name { get; }
        public Aggregator(Type aggregatorInstanceType, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo)
        {
            _aggregatorInstanceType = aggregatorInstanceType;
            _sourcePropertyInfo = sourcePropertyInfo;
            TargetPropertyInfo = targetPropertyInfo;
            _aggregationInstance = (IAggregationInstance)Activator.CreateInstance(aggregatorInstanceType);
            Name = targetPropertyInfo.Name;
        }
        public Aggregator<TIn> CopyEmpty()
        {
            return new Aggregator<TIn>(_aggregatorInstanceType, _sourcePropertyInfo, TargetPropertyInfo);
        }
        public void Aggregate(TIn input)
        {
            _aggregationInstance.Aggregate(_sourcePropertyInfo?.GetValue(input));
        }
        public object GetResult() => _aggregationInstance.GetResult();
    }
}
