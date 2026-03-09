using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Operators;

public class PushObservable
{
    public static IPushObservable<TOut> FromEnumerable<TOut>(IEnumerable<TOut> enumerable, WaitHandle startSynchronizer, CancellationToken cancellationToken)
    {
        return new EventDeferredPushObservable<TOut>((pushValue, ct) =>
        {
            foreach (var item in enumerable)
            {
                if (ct.IsCancellationRequested) break;
                pushValue(item);
            }
        }, startSynchronizer, cancellationToken);
    }
    public static IDeferredPushObservable<TOut> FromEnumerable<TOut>(IEnumerable<TOut> enumerable, CancellationToken cancellationToken)
    {
        return new DeferredPushObservable<TOut>((pushValue, ct) =>
        {
            foreach (var item in enumerable)
            {
                if (ct.IsCancellationRequested) break;
                pushValue(item);
            }
        }, cancellationToken);
    }
    public static IPushObservable<TOut> FromSingle<TOut>(TOut item, WaitHandle startSynchronizer, CancellationToken cancellationToken)
    {
        return new EventDeferredPushObservable<TOut>((pushValue, ct) =>
        {
            if (!ct.IsCancellationRequested)
                pushValue(item);
        }, startSynchronizer, cancellationToken);
    }
    public static IDeferredPushObservable<TOut> FromSingle<TOut>(TOut item, CancellationToken cancellationToken)
    {
        return new DeferredPushObservable<TOut>((pushValue, ct) => pushValue(item), cancellationToken);
    }
    public static IPushObservable<int> Range(int from, int count, WaitHandle startSynchronizer, CancellationToken cancellationToken)
    {
        return FromEnumerable(Enumerable.Range(from, count), startSynchronizer, cancellationToken);
    }
    public static IDeferredPushObservable<int> Range(int from, int count, CancellationToken cancellationToken)
    {
        return FromEnumerable(Enumerable.Range(from, count), cancellationToken);
    }
    public static IPushObservable<T> Merge<T>(params IPushObservable<T>[] pushObservables)
    {
        return new MergeSubject<T>(pushObservables);
    }
    public static IPushObservable<TOut> CombineWithLatest<TIn1, TIn2, TOut>(IPushObservable<TIn1> pushObservable1, IPushObservable<TIn2> pushObservable2, Func<TIn1, TIn2, TOut> selector)
    {
        return new CombineWithLatestSubject<TIn1, TIn2, TOut>(pushObservable1, pushObservable2, selector);
    }
    public static IPushObservable<TOut> Empty<TOut>(WaitHandle startSynchronizer, CancellationToken cancellationToken)
    {
        return new EventDeferredPushObservable<TOut>((i, ct) => { }, startSynchronizer, cancellationToken);
    }
    public static IDeferredPushObservable<TOut> Empty<TOut>(CancellationToken cancellationToken)
    {
        return new DeferredPushObservable<TOut>((i, ct) => { }, cancellationToken);
    }
}
