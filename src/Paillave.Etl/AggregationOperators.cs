using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;

namespace Paillave.Etl
{
    public static class AggregationOperators
    {
        public static TOut For<TOut>(this TOut aggregationResult, bool isSelected)
        {
            throw new NotSupportedException();
        }

        [AggregationInstance(typeof(CountNotNullAggregationInstance))]
        public static int Count<TOut>(TOut input)
        {
            throw new NotSupportedException();
        }

        [AggregationInstance(typeof(FirstAggregationInstance))]
        public static TOut First<TOut>(TOut input)
        {
            throw new NotSupportedException();
        }

        [AggregationInstance(typeof(FirstNotNullAggregationInstance))]
        public static TOut FirstNotNull<TOut>(TOut input)
        {
            throw new NotSupportedException();
        }

        [AggregationInstance(typeof(LastAggregationInstance))]
        public static TOut Last<TOut>(TOut input)
        {
            throw new NotSupportedException();
        }

        [AggregationInstance(typeof(CountAggregationInstance))]
        public static int Count()
        {
            throw new NotSupportedException();
        }

        [AggregationInstance(typeof(MaxAggregationInstance))]
        public static TOut Max<TOut>(TOut input) where TOut : IComparable<TOut>
        {
            throw new NotSupportedException();
        }

        [AggregationInstance(typeof(MinAggregationInstance))]
        public static TOut Min<TOut>(TOut input) where TOut : IComparable<TOut>
        {
            throw new NotSupportedException();
        }

        [AggregationInstance(typeof(SumAggregationInstance))]
        public static TOut Sum<TOut>(TOut input)
        {
            throw new NotSupportedException();
        }

        [AggregationInstance(typeof(AvgAggregationInstance))]
        public static TOut Avg<TOut>(TOut input)
        {
            throw new NotSupportedException();
        }
    }
}
