﻿using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Paillave.Etl.Reactive.Core
{
    public static class SortDefinition
    {
        public static SortDefinition<T, TKey> Create<T, TKey>(Func<T, TKey> getKey, object keyPosition = null) => new SortDefinition<T, TKey>(getKey, keyPosition);
    }
    public class SortDefinition<T, TKey> : IComparer<T>, IEqualityComparer<T>, System.Collections.IComparer, System.Collections.IEqualityComparer
    {
        internal List<SortStep> SortSteps { get; }

        public SortDefinition(Func<T, TKey> getKey, object keyPosition = null)
        {
            this.GetKey = getKey;
            this.KeyPosition = keyPosition;
            this.SortSteps = this.GetSortSteps();
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
            foreach (var item in SortSteps)
            {
                var xValue = (IComparable)item.GetValue(xKey);
                var yValue = (IComparable)item.GetValue(yKey);
                int cmp;
                if (item.SortOrder == SortOrder.Ascending)
                {
                    if (xValue != null)
                        cmp = xValue.CompareTo(yValue);
                    else if (yValue != null)
                        cmp = -yValue.CompareTo(xValue);
                    else
                        cmp = 0;
                }
                else
                {
                    if (yValue != null)
                        cmp = yValue.CompareTo(xValue);
                    else if (xValue != null)
                        cmp = -xValue.CompareTo(yValue);
                    else
                        cmp = 0;
                }
                //var cmp = item.SortOrder == SortOrder.Ascending ? xValue.CompareTo(yValue) : yValue.CompareTo(xValue);
                if (cmp != 0) return cmp;
            }
            return 0;
        }

        public bool Equals(T x, T y) => Compare(x, y) == 0;

        public int GetHashCode(T obj) => this.GetKey(obj)?.GetHashCode() ?? 0;
        private List<SortStep> GetSortSteps()
        {
            var keyType = typeof(TKey);
            if (typeof(IComparable).IsAssignableFrom(keyType))
            {
                SortOrder sortOrder = SortOrder.Ascending;
                switch (KeyPosition)
                {
                    case SortOrder so:
                        sortOrder = so;
                        break;
                    case int position:
                        sortOrder = position >= 0 ? SortOrder.Ascending : SortOrder.Descending;
                        break;
                }
                return new List<SortStep> { new SortStep(1, sortOrder, i => i) };
            }
            else if (KeyPosition == null)
                return typeof(TKey).GetProperties().Select((l, idx) => new SortStep(idx, SortOrder.Ascending, l))
                    .OrderBy(i => i.Position)
                    .ToList();
            else
                return typeof(TKey).GetProperties()
                    .Where(i => typeof(IComparable).IsAssignableFrom(i.PropertyType))
                    .Join(
                        this.KeyPosition.GetType().GetProperties().Select(i => new { i.Name, Position = (int)i.GetValue(KeyPosition) }),
                        i => i.Name,
                        i => i.Name,
                        (l, r) => new SortStep(Math.Abs(r.Position), r.Position >= 0 ? SortOrder.Ascending : SortOrder.Descending, l))
                    .OrderBy(i => i.Position)
                    .ToList();
        }

        int System.Collections.IComparer.Compare(object x, object y) => Compare((T)x, (T)y);

        bool System.Collections.IEqualityComparer.Equals(object x, object y) => Equals((T)x, (T)y);

        int System.Collections.IEqualityComparer.GetHashCode(object obj) => GetHashCode((T)obj);
    }
    public class SortStep(int position, SortOrder sortOrder)
    {
        public SortStep(int position, SortOrder sortOrder, PropertyInfo propertyInfo) : this(position, sortOrder) => 
            this.GetValue = (i) => propertyInfo.GetValue(i);
        public SortStep(int position, SortOrder sortOrder, Func<object, object> getValue) : this(position, sortOrder) => 
            this.GetValue = getValue;
        public Func<object, object> GetValue { get; }
        public int Position { get; } = position;
        public SortOrder SortOrder { get; } = sortOrder;
    }
}
