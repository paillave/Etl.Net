namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public abstract class AggregationInstanceBase : IAggregationInstance
    {
        public virtual void Aggregate(object value)
        {
            if (value == null) return;
            switch (value)
            {
                case int v: Add(v); break;
                case long v: Add(v); break;
                case decimal v: Add(v); break;
                case float v: Add(v); break;
                case double v: Add(v); break;
                case short v: Add(v); break;
                case byte v: Add(v); break;
                case uint v: Add(v); break;
                case ulong v: Add(v); break;
                case ushort v: Add(v); break;
            }
        }
        protected abstract void Add(int value);
        protected abstract void Add(long value);
        protected abstract void Add(decimal value);
        protected abstract void Add(float value);
        protected abstract void Add(double value);
        protected abstract void Add(short value);
        protected abstract void Add(byte value);
        protected abstract void Add(uint value);
        protected abstract void Add(ulong value);
        protected abstract void Add(ushort value);
        public abstract object GetResult();
    }
}