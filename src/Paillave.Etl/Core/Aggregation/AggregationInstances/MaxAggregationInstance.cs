using Paillave.Etl.Core.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class MaxAggregationInstance : IAggregationInstance
    {
        private object _max = null;
        public void Aggregate(object value)
        {
            var valueComp = value as IComparable;
            if (value != null && (_max == null || valueComp.CompareTo(_max) > 0)) _max = value;
        }
        public object GetResult()
        {
            return _max;
        }
    }
}
