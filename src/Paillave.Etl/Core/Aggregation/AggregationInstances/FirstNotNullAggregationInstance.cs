namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class FirstNotNullAggregationInstance : IAggregationInstance
    {
        private object? _first = null;
        private bool _hasValue = false;
        public void Aggregate(object? value)
        {
            if (!_hasValue && value != null)
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
