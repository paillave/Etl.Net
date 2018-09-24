using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class EventDeferedPushObservable<T> : PushObservableBase<T>
    {
        private bool _isComplete = false;
        private Action<Action<T>> _valuesFactory;
        private WaitHandle _startSynchronizer = null;
        private object lockObject = new object();
        public EventDeferedPushObservable(Action<Action<T>> valuesFactory, WaitHandle startSynchronizer)
        {
            _valuesFactory = valuesFactory;
            _startSynchronizer = startSynchronizer;
            this.Start();
        }
        private void Complete()
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
        private void Start()
        {
            Task.Run(() =>
            {
                this._startSynchronizer.WaitOne();
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
            });
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
