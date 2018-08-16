
using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Paillave.Etl.Core
{
    //public interface IComparer<T1, T2>
    //{
    //    int Compare(T1 x, T2 y);
    //}
    public class SortCriteriaComparer<T> : IComparer<T>
    {
        private readonly Func<T, T, int>[] _compareFunctions;

        public SortCriteriaComparer(IEnumerable<ISortCriteria<T>> sortCriterias)
        {
            if (sortCriterias.Count() == 0) throw new ArgumentOutOfRangeException(nameof(sortCriterias), "sorting criteria list cannot be empty");
            this._compareFunctions = sortCriterias.Select<ISortCriteria<T>, Func<T, T, int>>(i =>
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
    public class SortCriteriaComparer<T1, T2> : IComparer<T1, T2>
    {
        private readonly Func<T1, T2, int>[] _compareFunctions;

        public SortCriteriaComparer(IList<ISortCriteria<T1>> sortCriterias1, IList<ISortCriteria<T2>> sortCriterias2)
        {
            if (sortCriterias1.Count == 0) throw new ArgumentOutOfRangeException(nameof(sortCriterias1), "sorting criteria list 1 cannot be empty");
            if (sortCriterias2.Count == 0) throw new ArgumentOutOfRangeException(nameof(sortCriterias2), "sorting criteria list 2 cannot be empty");
            var tmp = Enumerable
                .Range(0, Math.Min(sortCriterias1.Count, sortCriterias2.Count))
                .Select(i => new Tuple<ISortCriteria<T1>, ISortCriteria<T2>>(sortCriterias1[i], sortCriterias2[i]))
                .ToList();
            foreach (var item in tmp)
            {
                if (item.Item1.Field.ReturnType != item.Item2.Field.ReturnType) throw new ArgumentException("both sort criterias must match");
                if (item.Item1.SortOrder != item.Item2.SortOrder) throw new ArgumentException("both sort criterias must match");
            }
            this._compareFunctions = tmp.Select<Tuple<ISortCriteria<T1>, ISortCriteria<T2>>, Func<T1, T2, int>>(i =>
            {
                var getter1 = i.Item1.Field.Compile();
                var getter2 = i.Item2.Field.Compile();
                if (i.Item1.SortOrder == SortOrder.Ascending)
                    return (a, b) => getter1(a).CompareTo(getter2(b));
                else
                    return (a, b) => getter2(b).CompareTo(getter1(a));
            }).ToArray();
        }
        public int Compare(T1 x, T2 y)
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
