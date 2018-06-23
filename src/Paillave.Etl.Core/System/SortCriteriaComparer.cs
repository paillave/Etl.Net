using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public class SortCriteriaComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int>[] _compareFunctions;

        public SortCriteriaComparer(params SortCriteria<T>[] sortCriterias)
        {
            if (sortCriterias.Length == 0) throw new ArgumentOutOfRangeException(nameof(sortCriterias), "sorting criteria list cannot be empty");
            this._compareFunctions = sortCriterias.Select<SortCriteria<T>, Func<T, T, int>>(i =>
            {
                var getter = i.Field.Compile();
                if (i.SortOrder == SortOrder.Ascending)
                    return (a, b) => getter(a).CompareTo(getter(b));
                else
                    return (a, b) => getter(b).CompareTo(getter(a));
            }).ToArray();
        }
        public int Compare(T x, T y)
        {
            if (y == null && x == null)
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;
            foreach (var item in _compareFunctions)
            {
                var cmp = item(x, y);
                if (cmp != 0) return cmp;
            }
            return 0;
        }
    }
}
