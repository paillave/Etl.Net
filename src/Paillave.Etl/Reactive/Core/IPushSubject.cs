using System;

namespace Paillave.Etl.Reactive.Core
{
    public interface IPushSubject<T> : IPushObservable<T>, IPushObserver<T>, IDisposable
    {
    }
}
