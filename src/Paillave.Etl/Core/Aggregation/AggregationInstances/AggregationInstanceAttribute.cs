using System;

namespace Paillave.Etl.Core.Aggregation.AggregationInstances;

[AttributeUsage(AttributeTargets.Method)]
public class AggregationInstanceAttribute(Type type) : Attribute
{
    public Type AggregationInstanceType { get; } = type;
}
