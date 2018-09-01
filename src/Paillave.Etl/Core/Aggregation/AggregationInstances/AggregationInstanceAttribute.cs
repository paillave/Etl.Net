using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
