using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class DeferredPushObservable<T> : PushObservableBase<T>, IDeferredPushObservable<T>
    {
        private bool _isComplete = false;
        private Action<Action<T>> _valuesFactory;
        private object lockObject = new object();
        public DeferredPushObservable(Action<Action<T>> valuesFactory)
        {
            _valuesFactory = valuesFactory;
        }
        private void Complete()
        {
            lock (lockObject)
            {
                this._isComplete = true;
                foreach (var item in base.Subscriptions.ToList())
                    item.OnComplete();
            }
        }

        public virtual void Dispose()
        {
            this.Complete();
        }

        public void PushException(Exception exception)
        {
            lock (lockObject)
            {
                foreach (var item in base.Subscriptions.ToList())
                    item.OnPushException(exception);
            }
        }

        private void PushValue(T value)
        {
            lock (lockObject)
            {
                foreach (var item in base.Subscriptions.ToList())
                    item.OnPushValue(value);
            }
        }

        private Guid tmp = Guid.NewGuid();
        public void Start()
        {
            Task.Run(() => InternStart());
        }
        private void InternStart()
        {
            lock (lockObject)
            {
                try
                {
                    _valuesFactory(PushValue);
                }
                catch (Exception ex)
                {
                    PushException(ex);
                }
                finally
                {
                    Complete();
                }
            }
        }

        public override IDisposable Subscribe(ISubscription<T> subscription)
        {
            lock (lockObject)
            {
                if (this._isComplete) subscription.OnComplete();
                var subs = base.Subscribe(subscription);
                return subs;
            }
        }
    }
}
