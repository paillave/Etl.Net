namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class AvgAggregationInstance : AggregationInstanceBase
    {
        private object _sum = null;
        private int _count = 0;
        protected override void Add(int value)
        {
            _count++;
            if (_sum == null) _sum = default(int);
            _sum = ((int)_sum) + value;
        }
        protected override void Add(long value)
        {
            _count++;
            if (_sum == null) _sum = default(long);
            _sum = ((long)_sum) + value;
        }
        protected override void Add(decimal value)
        {
            _count++;
            if (_sum == null) _sum = default(decimal);
            _sum = ((decimal)_sum) + value;
        }
        protected override void Add(float value)
        {
            _count++;
            if (_sum == null) _sum = default(float);
            _sum = ((float)_sum) + value;
        }
        protected override void Add(double value)
        {
            _count++;
            if (_sum == null) _sum = default(double);
            _sum = ((double)_sum) + value;
        }
        protected override void Add(short value)
        {
            _count++;
            if (_sum == null) _sum = default(short);
            _sum = ((short)_sum) + value;
        }
        protected override void Add(byte value)
        {
            _count++;
            if (_sum == null) _sum = default(byte);
            _sum = ((byte)_sum) + value;
        }
        protected override void Add(uint value)
        {
            _count++;
            if (_sum == null) _sum = default(uint);
            _sum = ((uint)_sum) + value;
        }
        protected override void Add(ulong value)
        {
            _count++;
            if (_sum == null) _sum = default(ulong);
            _sum = ((ulong)_sum) + value;
        }
        protected override void Add(ushort value)
        {
            _count++;
            if (_sum == null) _sum = default(ushort);
            _sum = ((ushort)_sum) + value;
        }
        public override object GetResult()
        {
            //return _sum;
            if (_sum == null) return null;
            switch (_sum)
            {
                case int v: return GetResult(v);
                case long v: return GetResult(v);
                case decimal v: return GetResult(v);
                case float v: return GetResult(v);
                case double v: return GetResult(v);
                case short v: return GetResult(v);
                case byte v: return GetResult(v);
                case uint v: return GetResult(v);
                case ushort v: return GetResult(v);
                default: return null;
            }
        }

        private int GetResult(int value) => value / _count;
        private long GetResult(long value) => value / _count;
        private decimal GetResult(decimal value) => value / _count;
        private float GetResult(float value) => value / _count;
        private double GetResult(double value) => value / _count;
    }
}
