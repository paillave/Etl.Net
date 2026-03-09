using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Paillave.Etl.Reactive.Operators;

public class ExceptionOnUnsortedSubject<T> : FilterSubjectBase<T>
{
    private readonly IComparer<T> _comparer;
    private T _lastValue = default(T);
    private bool _hasLastValue = false;
    private readonly object _syncValue = new();
    private readonly bool _distinctItems = false;
    public ExceptionOnUnsortedSubject(IPushObservable<T> observable, IComparer<T> comparer, bool distinctItems = false) : base(observable)
    {
        lock (_syncValue)
        {
            _comparer = comparer;
            _distinctItems = distinctItems;
        }
    }

    protected override bool AcceptsValue(T value)
    {
        lock (_syncValue)
        {
            if (_hasLastValue)
            {
                int comparison = _comparer.Compare(_lastValue, value);
                if (_distinctItems && comparison == 0)
                {
                    PushException(new NotDistinctlySortedObservableException());
                    return false;
                }
                else if (comparison > 0)
                {
                    PushException(new NotSortedObservableException());
                    return false;
                }
            }
            _lastValue = value;
            _hasLastValue = true;
            return true;
        }
    }
}
public class NotSortedObservableException : Exception
{

}
public class NotDistinctlySortedObservableException : Exception
{

}
public static partial class ObservableExtensions
{
    public static IPushObservable<T> ExceptionOnUnsorted<T>(this IPushObservable<T> observable, Func<T, IComparable> key, object keyPosition = null, bool distinctItems = false)
    {
        return new ExceptionOnUnsortedSubject<T>(observable, SortDefinition.Create(key, keyPosition), distinctItems);
    }
    public static IPushObservable<T> ExceptionOnUnsorted<T>(this IPushObservable<T> observable, IComparer<T> comparer, bool distinctItems = false)
    {
        return new ExceptionOnUnsortedSubject<T>(observable, comparer, distinctItems);
    }
}
