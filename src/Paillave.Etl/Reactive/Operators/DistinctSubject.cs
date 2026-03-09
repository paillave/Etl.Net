using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators;

public class DistinctSubject<T> : FilterSubjectBase<T>
{
    private readonly IEqualityComparer<T> _comparer;
    private readonly IList<T> _passedItems = new List<T>();
    private readonly object _syncValue = new();
    public DistinctSubject(IPushObservable<T> observable, IEqualityComparer<T> comparer) : base(observable)
    {
        lock (_syncValue)
        {
            _comparer = comparer;
        }
    }

    protected override bool AcceptsValue(T value)
    {
        lock (_syncValue)
        {
            if (!_passedItems.Contains(value, _comparer))
            {
                _passedItems.Add(value);
                return true;
            }
            return false;
        }
    }
}
public class DistinctSubject<T, K> : FilterSubjectBase<T>
{
    private readonly HashSet<K> _passedKeys = new();
    private readonly Func<T, K> _getKey;
    private readonly object _syncValue = new();
    public DistinctSubject(IPushObservable<T> observable, Func<T, K> getKey) : base(observable)
    {
        lock (_syncValue)
        {
            _getKey = getKey;
        }
    }

    protected override bool AcceptsValue(T value)
    {
        lock (_syncValue)
        {
            K key = _getKey(value);
            if (!_passedKeys.Contains(key))
            {
                _passedKeys.Add(key);
                return true;
            }
            return false;
        }
    }
}
public static partial class ObservableExtensions
{
    public static IPushObservable<T> Distinct<T>(this IPushObservable<T> observable, IEqualityComparer<T> comparer)
    {
        return new DistinctSubject<T>(observable, comparer);
    }
    public static IPushObservable<T> Distinct<T>(this IPushObservable<T> observable, Func<T, T, bool> comparer)
    {
        return new DistinctSubject<T>(observable, new LambdaEqualityComparer<T>(comparer));
    }
    public static IPushObservable<T> Distinct<T, K>(this IPushObservable<T> observable, Func<T, K> getKey)
    {
        return new DistinctSubject<T, K>(observable, getKey);
    }
    public static IPushObservable<T> Distinct<T>(this IPushObservable<T> observable) where T : IEquatable<T>
    {
        return new DistinctSubject<T>(observable, new LambdaEqualityComparer<T>((l, r) => l.Equals(r)));
    }
}
