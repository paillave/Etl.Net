namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public interface IAggregationInstance
    {
        void Aggregate(object value);
        object GetResult();
    }
}
