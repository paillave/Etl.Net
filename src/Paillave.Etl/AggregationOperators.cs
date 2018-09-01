using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    public static class AggregationOperators
    {
        [AggregationInstance(typeof(CountNotNullAggregationInstance))]
        public static int Count<TOut>(TOut input)
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
