using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public abstract class PushObservableBase<T> : IPushObservable<T>
    {
        protected IList<ISubscription<T>> Subscriptions { get; } = new List<ISubscription<T>>();

        public virtual IDisposable Subscribe(ISubscription<T> subscription)
        {
            this.Subscriptions.Add(subscription);
            return new Unsubscriber(this, subscription);
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
