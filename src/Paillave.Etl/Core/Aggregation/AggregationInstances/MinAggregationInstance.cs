using System;

namespace Paillave.Etl.Core.Aggregation.AggregationInstances;

public class MinAggregationInstance : IAggregationInstance
{
    private object? _min = null;
    public void Aggregate(object? value)
    {
        var valueComp = value as IComparable;
        if (valueComp != null && (_min == null || valueComp.CompareTo(_min) < 0)) _min = value;
    }
    public object? GetResult()
    {
        return _min;
    }
}
