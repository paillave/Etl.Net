using Paillave.Etl.Core.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.Aggregation.AggregationInstances
{
    public class SumAggregationInstance : IAggregationInstance
    {
        private object _sum = null;
        public void Aggregate(object value)
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
        private void Add(int value)
        {
            if (_sum == null) _sum = default(int);
            _sum = ((int)_sum) + value;
        }
        private void Add(long value)
        {
            if (_sum == null) _sum = default(long);
            _sum = ((long)_sum) + value;
        }
        private void Add(decimal value)
        {
            if (_sum == null) _sum = default(decimal);
            _sum = ((decimal)_sum) + value;
        }
        private void Add(float value)
        {
            if (_sum == null) _sum = default(float);
            _sum = ((float)_sum) + value;
        }
        private void Add(double value)
        {
            if (_sum == null) _sum = default(double);
            _sum = ((double)_sum) + value;
        }
        private void Add(short value)
        {
            if (_sum == null) _sum = default(short);
            _sum = ((short)_sum) + value;
        }
        private void Add(byte value)
        {
            if (_sum == null) _sum = default(byte);
            _sum = ((byte)_sum) + value;
        }
        private void Add(uint value)
        {
            if (_sum == null) _sum = default(uint);
            _sum = ((uint)_sum) + value;
        }
        private void Add(ulong value)
        {
            if (_sum == null) _sum = default(ulong);
            _sum = ((ulong)_sum) + value;
        }
        private void Add(ushort value)
        {
            if (_sum == null) _sum = default(ushort);
            _sum = ((ushort)_sum) + value;
        }
        public object GetResult()
        {
            return _sum;
        }
    }
}
