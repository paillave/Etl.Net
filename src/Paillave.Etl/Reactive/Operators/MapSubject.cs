using System;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators;

public class MapSubject<TIn, TOut> : PushSubject<TOut>
{
    private readonly IDisposable _subscription;
    public MapSubject(IPushObservable<TIn> observable, Func<TIn, TOut> selector) : base(observable.CancellationToken)
    {
        this._subscription = observable.Subscribe(i => this.TryPushValue(() => selector(i)), this.Complete, this.PushException);
    }
    public MapSubject(IPushObservable<TIn> observable, Func<TIn, int, TOut> selector) : base(observable.CancellationToken)
    {
        int counter = 0;
        this._subscription = observable.Subscribe(i => this.TryPushValue(() => selector(i, counter++)), this.Complete, this.PushException);
    }
    public override void Dispose()
    {
        base.Dispose();
        _subscription.Dispose();
    }
}
public static partial class ObservableExtensions
{
    public static IPushObservable<TOut> Map<TIn, TOut>(this IPushObservable<TIn> observable, Func<TIn, TOut> selector)
        => new MapSubject<TIn, TOut>(observable, selector);
    public static IPushObservable<TOut> Map<TIn, TOut>(this IPushObservable<TIn> observable, Func<TIn, int, TOut> selector)
        => new MapSubject<TIn, TOut>(observable, selector);
}
