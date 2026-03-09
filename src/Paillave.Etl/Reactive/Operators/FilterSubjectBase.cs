using System;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators;

public abstract class FilterSubjectBase<T> : PushSubject<T>
{
    private readonly IDisposable _subscription;

    protected abstract bool AcceptsValue(T value);
    private readonly object _syncValue = new();

    public FilterSubjectBase(IPushObservable<T> observable) : base(observable.CancellationToken)
    {
        this._subscription = observable.Subscribe(HandlePushValue, this.Complete, this.PushException);
    }

    private void HandlePushValue(T value)
    {
        if (CancellationToken.IsCancellationRequested)
        {
            return;
        }
        lock (_syncValue)
        {
            try
            {
                if (AcceptsValue(value))
                    this.PushValue(value);
            }
            catch (Exception ex)
            {
                this.PushException(ex);
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _subscription.Dispose();
    }
}
