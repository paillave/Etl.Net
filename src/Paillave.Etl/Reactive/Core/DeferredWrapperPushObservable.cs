using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.Reactive.Core
{
    public class DeferredWrapperPushObservable<T>(IPushObservable<T> observable, Action start, Synchronizer synchronizer, CancellationToken cancellationToken) : IDeferredPushObservable<T>
    {
        public CancellationToken CancellationToken { get; } = cancellationToken;
        private Action _start = start;
        private IPushObservable<T> _observable = observable;
        private Synchronizer _synchronizer = synchronizer;

        public void Start()
        {
            if (_synchronizer == null)
                _start();
            else
                using (_synchronizer.WaitBeforeProcess())
                    _start();
        }
        public IDisposable Subscribe(Action<T> onPush) => _observable.Subscribe(onPush);
        public IDisposable Subscribe(Action<T> onPush, Action onComplete) => _observable.Subscribe(onPush, onComplete);
        public IDisposable Subscribe(Action<T> onPush, Action onComplete, Action<Exception> onException) => _observable.Subscribe(onPush, onComplete, onException);
        public IDisposable Subscribe(ISubscription<T> subscription) => _observable.Subscribe(subscription);
    }
}
