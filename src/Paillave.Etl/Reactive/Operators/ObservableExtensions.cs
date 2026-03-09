using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators;

public static partial class ObservableExtensions
{
    public static Task<T> LastAsync<T>(this IPushObservable<T> observable) => ToTaskAsync(observable);
    public static Task<T> ToTaskAsync<T>(this IPushObservable<T> observable)
    {
        T latestValue = default(T);
        var tcs = new TaskCompletionSource<T>();
        var subscription = observable.Subscribe(v => latestValue = v, () => tcs.SetResult(latestValue));
        return tcs.Task.ContinueWith(i =>
        {
            subscription.Dispose();
            return i.Result;
        });
    }
    public static Task ToEndAsync<T>(this IPushObservable<T> observable)
    {
        var tcs = new TaskCompletionSource<T>();
        var subscription = observable.Subscribe(v => { }, () => tcs.SetResult(default));
        return tcs.Task.ContinueWith(i => subscription.Dispose());
    }
    public static IPushObservable<T> DelayTillEndOfStream<T>(this IPushObservable<T> observable)
        => observable.ToList().FlatMap((i, ct) => PushObservable.FromEnumerable(i, ct));
    public static Task<List<T>> ToListAsync<T>(this IPushObservable<T> observable)
        => observable.ToList().ToTaskAsync();
    public static IPushObservable<int> Count<T>(this IPushObservable<T> observable)
        => observable.Aggregate<T, int>((acc, red) => acc + 1);
    public static IPushObservable<int> Count<T>(this IPushObservable<T> observable, Func<T, bool> criteria)
        => observable.Filter(criteria).Aggregate<T, int>((acc, red) => acc + 1);
    public static IPushObservable<TResult[]> PairWithPrevious<TResult>(this IPushObservable<TResult> sourceS, int count)
        => sourceS.Scan<TResult, TResult[]>(null, (a, v) => new TResult[] { v, a == null ? default : a[0] });
}
