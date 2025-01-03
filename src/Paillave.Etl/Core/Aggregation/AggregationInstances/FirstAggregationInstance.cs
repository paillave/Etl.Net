namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class FirstAggregationInstance : IAggregationInstance
    {
        private object? _first = null;
        private bool _hasValue = false;
        public void Aggregate(object? value)
        {
            if (!_hasValue)
            {
                _first = value;
                _hasValue = true;
            }
        }
        public object? GetResult()
        {
            return _first;
        }
    }
}
