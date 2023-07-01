namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class CountNotNullAggregationInstance : IAggregationInstance
    {
        private int _count = 0;
        public void Aggregate(object value)
        {
            if (value != null) _count++;
        }
        public object GetResult()
        {
            return _count;
        }
    }
}
