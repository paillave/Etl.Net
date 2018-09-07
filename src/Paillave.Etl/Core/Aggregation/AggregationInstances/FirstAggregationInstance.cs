using Paillave.Etl.Core.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class FirstAggregationInstance : IAggregationInstance
    {
        private object _first = null;
        private bool _hasValue = false;
        public void Aggregate(object value)
        {
            if (!_hasValue)
            {
                _first = value;
                _hasValue = true;
            }
        }
        public object GetResult()
        {
            return _first;
        }
    }
}
