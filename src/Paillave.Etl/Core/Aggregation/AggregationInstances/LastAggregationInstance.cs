using Paillave.Etl.Core.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class LastAggregationInstance : IAggregationInstance
    {
        private object _last = null;
        public void Aggregate(object value)
        {
            _last = value;
        }
        public object GetResult()
        {
            return _last;
        }
    }
}
