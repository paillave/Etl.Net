using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public abstract class PushObservableBase<T> : IPushSubject<T>
    {
        private bool _isComplete = false;
        protected object LockObject = new object();
        protected IList<ISubscription<T>> Subscriptions { get; } = new List<ISubscription<T>>();

        public virtual IDisposable Subscribe(ISubscription<T> subscription)
        {
            lock (LockObject)
            {
                if (this._isComplete) subscription.OnComplete();
                this.Subscriptions.Add(subscription);
                return new Unsubscriber(this, subscription);
            }
        }
        protected bool IsComplete
        {
            get
            {
                lock (LockObject)
                    return _isComplete;
            }
        }
        public void Complete()
        {
            lock (LockObject)
            {
                if (this._isComplete) return;
                this._isComplete = true;
                foreach (var item in this.Subscriptions.ToList())
                    item.OnComplete();
            }
        }

        public virtual void Dispose()
        {
            this.Complete();
        }

        public void PushException(Exception exception)
        {
            lock (LockObject)
            {
                if (!this._isComplete)
                    foreach (var item in this.Subscriptions.ToList())
                        item.OnPushException(exception);
            }
        }

        public void PushValue(T value)
        {
            lock (LockObject)
            {
                if (!this._isComplete)
                    foreach (var item in this.Subscriptions.ToList())
                        item.OnPushValue(value);
            }
        }

        public IDisposable Subscribe(Action<T> onPush)
        {
            return Subscribe(new Subscription<T>(onPush));
        }

        public IDisposable Subscribe(Action<T> onPush, Action onComplete)
        {
            return Subscribe(new Subscription<T>(onPush, onComplete));
        }

        public IDisposable Subscribe(Action<T> onPush, Action onComplete, Action<Exception> onException)
        {
            return Subscribe(new Subscription<T>(onPush, onComplete, onException));
        }
        // public override IDisposable Subscribe(ISubscription<T> subscription)
        // {
        //     lock (lockObject)
        //     {
        //         if (this._isComplete) subscription.OnComplete();
        //         return this.Subscribe(subscription);
        //     }
        // }

        private class Unsubscriber : IDisposable
        {
            private PushObservableBase<T> _observableBase;
            private ISubscription<T> _subscription;

            public Unsubscriber(PushObservableBase<T> observableBase, ISubscription<T> subscription)
            {
                this._observableBase = observableBase;
                this._subscription = subscription;
            }

            public void Dispose()
            {
                this._observableBase.Subscriptions.Remove(this._subscription);
            }
        }
    }
}
