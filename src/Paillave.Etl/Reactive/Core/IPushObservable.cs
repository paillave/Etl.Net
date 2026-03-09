using System;
using System.Threading;

namespace Paillave.Etl.Reactive.Core;

public interface IPushObservable
{
    CancellationToken CancellationToken { get; }
}
public interface IPushObservable<out T> : IPushObservable
{
    IDisposable Subscribe(Action<T> onPush);
    IDisposable Subscribe(Action<T> onPush, Action onComplete);
    IDisposable Subscribe(Action<T> onPush, Action onComplete, Action<Exception> onException);
    IDisposable Subscribe(ISubscription<T> subscription);
}
