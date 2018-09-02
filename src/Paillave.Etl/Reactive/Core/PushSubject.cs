using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class PushSubject<T> : PushObservableBase<T>, IPushSubject<T>
    {
        private bool _isComplete = false;
        private object lockObject = new object();

        protected bool IsComplete
        {
            get
            {
                lock (lockObject)
                    return _isComplete;
            }
        }

        public void Complete()
        {
            lock (lockObject)
            {
                foreach (var item in base.Subscriptions.ToList())
                    item.OnComplete();
                this._isComplete = true;
            }
        }

        public virtual void Dispose()
        {
            lock (lockObject)
                if (!this._isComplete)
                    this.Complete();
        }

        public void PushException(Exception exception)
        {
            lock (lockObject)
            {
                if (!this._isComplete)
                    foreach (var item in base.Subscriptions.ToList())
                        item.OnPushException(exception);
            }
        }

        public void PushValue(T value)
        {
            lock (lockObject)
            {
                if (!this._isComplete)
                    foreach (var item in base.Subscriptions.ToList())
                        item.OnPushValue(value);
            }
        }
        public override IDisposable Subscribe(ISubscription<T> subscription)
        {
            lock (lockObject)
            {
                if (this._isComplete) subscription.OnComplete();
                return base.Subscribe(subscription);
            }
        }
    }
}
