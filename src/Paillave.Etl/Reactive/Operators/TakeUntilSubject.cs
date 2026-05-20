using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Operators;

public class TakeUntilSubject<TIn, TFrom> : PushSubject<TIn>
{
    private readonly object _lockObject = new();
    private readonly IDisposable _disp1;
    private readonly IDisposable _disp2;
    private readonly CancellationTokenSource _linkedCts;

    public TakeUntilSubject(IPushObservable<TIn> observable, IPushObservable<TFrom> fromObservable)
        : this(CancellationTokenSource.CreateLinkedTokenSource(observable.CancellationToken, fromObservable.CancellationToken), observable, fromObservable) { }

    private TakeUntilSubject(CancellationTokenSource linkedCts, IPushObservable<TIn> observable, IPushObservable<TFrom> fromObservable) : base(linkedCts.Token)
    {
        _linkedCts = linkedCts;
        _disp1 = observable.Subscribe(PushValue, Complete, PushException);
        _disp2 = fromObservable.Subscribe(HandleOnPushTrigger);
    }

    private void HandleOnPushTrigger(TFrom obj)
    {
        lock (_lockObject)
        {
            Complete();
        }
    }

    protected override void OnCompleted()
    {
        _disp1?.Dispose();
        _disp2?.Dispose();
        _linkedCts?.Dispose();
    }

    public override void Dispose()
    {
        _disp1.Dispose();
        _disp2.Dispose();
        _linkedCts?.Dispose();
        base.Dispose();
    }
}
public static partial class ObservableExtensions
{
    public static IPushObservable<TIn> TakeUntil<TIn, TFrom>(this IPushObservable<TIn> observable, IPushObservable<TFrom> fromObservable)
    {
        return new TakeUntilSubject<TIn, TFrom>(observable, fromObservable);
    }
}
