using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {
        public Func<T, T, bool> Comparer { get; }
        public LambdaEqualityComparer(Func<T, T, bool> comparer)
        {
            Comparer = comparer;
        }
        public bool Equals(T x, T y)
        {
            return Comparer(x, y);
        }
        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
    public class LambdaEqualityComparer<T, K> : IEqualityComparer<T> where K : IEquatable<K>
    {
        private readonly Func<T, K> _propertyToCompare;
        public Func<T, T, bool> Comparer { get; }
        public LambdaEqualityComparer(Func<T, K> prop)
        {
            _propertyToCompare = prop;
            Comparer = (t1, t2) => _propertyToCompare(t1).Equals(_propertyToCompare(t2));
        }
        public bool Equals(T x, T y)
        {
            return Comparer(x, y);
        }
        public int GetHashCode(T obj)
        {
            return _propertyToCompare(obj)?.GetHashCode() ?? 0;
        }
    }
}
