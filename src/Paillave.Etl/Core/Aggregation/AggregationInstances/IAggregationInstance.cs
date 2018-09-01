using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public interface IAggregationInstance
    {
        void Aggregate(object value);
        object GetResult();
    }
}
