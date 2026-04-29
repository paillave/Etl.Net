using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators;

public class TakeSubject<T> : PushSubject<T>
{
    private readonly IDisposable _subscription;
    private readonly object _lockObject = new();

    public TakeSubject(IPushObservable<T> observable, int count) : base(observable.CancellationToken)
    {
        this._subscription = observable.Subscribe(i =>
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_lockObject)
            {
                count--;
                if (count >= 0) this.PushValue(i);
                if (count == 0) this.Complete();
            }
        }, this.Complete, (ex) =>
        {
            lock (_lockObject)
            {
                if (count >= 0) this.PushException(ex);
            }
        });
    }

    protected override void OnCompleted()
    {
        // Stop pulling from upstream as soon as we've taken enough; otherwise
        // the upstream chain stays rooted via its subscription list.
        _subscription.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();
        _subscription.Dispose();
    }
}
public static partial class ObservableExtensions
{
    public static IPushObservable<T> Take<T>(this IPushObservable<T> observable, int count)
    {
        return new TakeSubject<T>(observable, count);
    }
}
