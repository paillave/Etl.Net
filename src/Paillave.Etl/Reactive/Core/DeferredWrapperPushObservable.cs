using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Reactive.Core
{
    public class DeferredWrapperPushObservable<T> : IDeferredPushObservable<T>
    {
        private Action _start;
        private IPushObservable<T> _observable;
        public DeferredWrapperPushObservable(IPushObservable<T> observable, Action start)
        {
            _observable = observable;
            _start = start;
        }
        public void Start()
        {
            _start();
        }

        public IDisposable Subscribe(Action<T> onPush)
        {
            return _observable.Subscribe(onPush);
        }

        public IDisposable Subscribe(Action<T> onPush, Action onComplete)
        {
            return _observable.Subscribe(onPush, onComplete);
        }

        public IDisposable Subscribe(Action<T> onPush, Action onComplete, Action<Exception> onException)
        {
            return _observable.Subscribe(onPush, onComplete, onException);
        }

        public IDisposable Subscribe(ISubscription<T> subscription)
        {
            return _observable.Subscribe(subscription);
        }
    }
}
