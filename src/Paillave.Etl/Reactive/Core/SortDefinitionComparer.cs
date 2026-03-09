using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core;

public class SortDefinitionComparer<T1, T2, TKey> : IComparer<T1, T2>
{
    private readonly List<SortStep> _sortSteps;
    private readonly Func<T1, TKey> _getKey1;
    private readonly Func<T2, TKey> _getKey2;

    public SortDefinitionComparer(SortDefinition<T1, TKey> sortDefinition1, SortDefinition<T2, TKey> sortDefinition2)
    {
        if (sortDefinition1 == null) throw new ArgumentNullException(nameof(sortDefinition1));
        if (sortDefinition2 == null) throw new ArgumentOutOfRangeException(nameof(sortDefinition2));
        if (!(sortDefinition1.KeyPosition == null && sortDefinition2.KeyPosition == null))
        {
            if (sortDefinition1.KeyPosition == null && sortDefinition2.KeyPosition != null)
                throw new ArgumentException("the key must match exactly between both sort definitions", nameof(sortDefinition2));
            else if (sortDefinition1.KeyPosition != null && sortDefinition2.KeyPosition == null)
                throw new ArgumentException("the key must match exactly between both sort definitions", nameof(sortDefinition2));
            else if (!sortDefinition1.KeyPosition.Equals(sortDefinition2.KeyPosition))
                throw new ArgumentException("the key must match exactly between both sort definitions", nameof(sortDefinition2));
        }

        _sortSteps = sortDefinition1.SortSteps; //it could as well be for sortDefinition2 as they both refer to the same key type and because the sort directions have been just checked to be equals
        _getKey1 = sortDefinition1.GetKey;
        _getKey2 = sortDefinition2.GetKey;
    }
    public int Compare(T1 x, T2 y)
    {
        if (y == null && x == null)
            return 0;
        else if (x == null)
            return -1;
        else if (y == null)
            return 1;
        TKey xKey = _getKey1(x);
        TKey yKey = _getKey2(y);
        foreach (var item in _sortSteps)
        {
            var xValue = (IComparable)item.GetValue(xKey);
            var yValue = (IComparable)item.GetValue(yKey);
            var cmp = item.SortOrder == SortOrder.Ascending ? xValue.CompareTo(yValue) : yValue.CompareTo(xValue);
            if (cmp != 0) return cmp;
        }
        return 0;
    }
}
