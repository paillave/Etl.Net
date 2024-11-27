using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave
{
    public class LambdaEqualityComparer<T>(Func<T, T, bool> comparer) : IEqualityComparer<T>
    {
        public Func<T, T, bool> Comparer { get; } = comparer;

        public bool Equals(T x, T y) => Comparer(x, y);
        public int GetHashCode(T obj) => 0;
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
        public bool Equals(T x, T y) => Comparer(x, y);
        public int GetHashCode(T obj) => _propertyToCompare(obj)?.GetHashCode() ?? 0;
    }
}
