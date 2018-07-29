using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.RxPush.Core
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
            return obj.GetHashCode();
        }
    }
}
