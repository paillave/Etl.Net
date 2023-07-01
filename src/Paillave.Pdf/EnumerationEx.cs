using System;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Pdf
{
    public static class EnumerationEx
    {
        public static IEnumerable<(T, T)> Pair<T>(this IEnumerable<T> range)
        {
            bool hasFirstElement = false;
            T previousElement = default;
            foreach (var item in range)
            {
                if (hasFirstElement) yield return (previousElement, item);
                else hasFirstElement = true;
                previousElement = item;
            }
        }
        public static (int, int) GetPosition<T>(this IList<T> range, T elt) where T : IComparable<T>
            => GetPosition(range, elt, new LambdaComparer<T>((x, y) => x.CompareTo(y)));
        public static (int, int) GetPosition<T>(this IList<T> range, T elt, Func<T, T, int> compare)
            => GetPosition(range, elt, new LambdaComparer<T>((x, y) => compare(x, y)));
        public static (int, int) GetPosition<T>(this IList<T> range, T elt, IComparer<T> comparer)
        {
            int leftBound = 0;
            int rightBound = range.Count - 1;
            while (leftBound < rightBound - 1)
            {
                int middleBound = (leftBound + rightBound) / 2;
                if (comparer.Compare(range[middleBound], elt) > 0) rightBound = middleBound;
                else leftBound = middleBound;
            }
            return (leftBound, rightBound);
        }
        public static bool Contains(this IEnumerable<double> range, double value, double proximity)
            => proximity == 0 ? range.Contains(value) : range.Any(i => value - proximity <= i && value + proximity >= i);
        private class LambdaComparer<T> : IComparer<T>
        {
            private readonly Func<T, T, int> _compare;
            public LambdaComparer(Func<T, T, int> compare) => (_compare) = (compare);
            public int Compare(T x, T y) => _compare(x, y);
        }
    }
}