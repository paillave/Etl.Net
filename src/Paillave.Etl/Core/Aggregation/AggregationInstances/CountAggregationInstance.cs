namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class CountAggregationInstance : IAggregationInstance
    {
        private int _count = 0;
        public void Aggregate(object? value)
        {
            _count++;
        }
        public object GetResult()
        {
            return _count;
        }
    }
}
