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
            return obj?.GetHashCode() ?? 0;
        }
    }
    public class LambdaEqualityComparer<T, K> : IEqualityComparer<T> where K : IEquatable<K>
    {
        public Func<T, T, bool> Comparer { get; }
        public LambdaEqualityComparer(Func<T, K> prop)
        {
            Comparer = (t1, t2) => prop(t1).Equals(prop(t2));
        }
        public bool Equals(T x, T y)
        {
            return Comparer(x, y);
        }
        public int GetHashCode(T obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}
