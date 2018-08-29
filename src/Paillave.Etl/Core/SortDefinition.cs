using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Paillave.Etl.Core
{
    public class SortDefinition<T, TKey> : IComparer<T>, IEqualityComparer<T>, System.Collections.IComparer, System.Collections.IEqualityComparer
    {
        private List<SortCriteria> _sortCriterias;

        private class SortCriteria
        {
            public int Position { get; set; }
            public SortOrder SortOrder { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
        }
        public SortDefinition(Func<T, TKey> getKey, object keyPosition)
        {
            this.GetKey = getKey;
            this.KeyPosition = keyPosition;
            this._sortCriterias = this.GetSortCriterias();
        }
        public Func<T, TKey> GetKey { get; }
        public object KeyPosition { get; }

        public int Compare(T x, T y)
        {
            if (y == null && x == null)
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;
            TKey xKey = GetKey(x);
            TKey yKey = GetKey(y);
            foreach (var item in _sortCriterias)
            {
                var xValue = (IComparable)item.PropertyInfo.GetValue(xKey);
                var yValue = (IComparable)item.PropertyInfo.GetValue(yKey);
                var cmp = item.SortOrder == SortOrder.Ascending ? xValue.CompareTo(yValue) : yValue.CompareTo(xValue);
                if (cmp != 0) return cmp;
            }
            return 0;
        }

        public bool Equals(T x, T y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(T obj)
        {
            return this.GetKey(obj).GetHashCode();
        }
        private List<SortCriteria> GetSortCriterias()
        {
            return typeof(TKey).GetProperties()
                .Where(i => typeof(IComparable).IsAssignableFrom(i.PropertyType))
                .Join(
                    this.KeyPosition.GetType().GetProperties().Select(i => new { i.Name, Position = (int)i.GetValue(KeyPosition) }),
                    i => i.Name,
                    i => i.Name,
                    (l, r) => new
                    SortCriteria
                    {
                        PropertyInfo = l,
                        SortOrder = r.Position >= 0 ? SortOrder.Ascending : SortOrder.Descending,
                        Position = Math.Abs(r.Position)
                    })
                .OrderBy(i => i.Position)
                .ToList();
        }

        int System.Collections.IComparer.Compare(object x, object y)
        {
            return Compare((T)x, (T)y);
        }

        bool System.Collections.IEqualityComparer.Equals(object x, object y)
        {
            return Equals((T)x, (T)y);
        }

        int System.Collections.IEqualityComparer.GetHashCode(object obj)
        {
            return GetHashCode((T)obj);
        }
    }
}
