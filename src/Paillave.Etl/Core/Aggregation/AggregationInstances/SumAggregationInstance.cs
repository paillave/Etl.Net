namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class SumAggregationInstance : AggregationInstanceBase
    {
        private object _sum = null;
        protected override void Add(int value)
        {
            if (_sum == null) _sum = default(int);
            _sum = ((int)_sum) + value;
        }
        protected override void Add(long value)
        {
            if (_sum == null) _sum = default(long);
            _sum = ((long)_sum) + value;
        }
        protected override void Add(decimal value)
        {
            if (_sum == null) _sum = default(decimal);
            _sum = ((decimal)_sum) + value;
        }
        protected override void Add(float value)
        {
            if (_sum == null) _sum = default(float);
            _sum = ((float)_sum) + value;
        }
        protected override void Add(double value)
        {
            if (_sum == null) _sum = default(double);
            _sum = ((double)_sum) + value;
        }
        protected override void Add(short value)
        {
            if (_sum == null) _sum = default(short);
            _sum = ((short)_sum) + value;
        }
        protected override void Add(byte value)
        {
            if (_sum == null) _sum = default(byte);
            _sum = ((byte)_sum) + value;
        }
        protected override void Add(uint value)
        {
            if (_sum == null) _sum = default(uint);
            _sum = ((uint)_sum) + value;
        }
        protected override void Add(ulong value)
        {
            if (_sum == null) _sum = default(ulong);
            _sum = ((ulong)_sum) + value;
        }
        protected override void Add(ushort value)
        {
            if (_sum == null) _sum = default(ushort);
            _sum = ((ushort)_sum) + value;
        }
        public override object GetResult()
        {
            return _sum;
        }
    }
}
