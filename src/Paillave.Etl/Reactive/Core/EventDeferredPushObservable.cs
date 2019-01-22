using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class EventDeferredPushObservable<T> : PushObservableBase<T>
    {
        private Action<Action<T>> _valuesFactory;
        private WaitHandle _startSynchronizer = null;
        public EventDeferredPushObservable(Action<Action<T>> valuesFactory, WaitHandle startSynchronizer)
        {
            _valuesFactory = valuesFactory;
            _startSynchronizer = startSynchronizer;
            this.Start();
        }

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
    }
}
