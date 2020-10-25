using System;

namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AggregationInstanceAttribute : Attribute
    {
        public Type AggregationInstanceType { get; }
        public AggregationInstanceAttribute(Type type)
        {
            this.AggregationInstanceType = type;
        }
    }
}
