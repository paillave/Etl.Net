using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class EventDeferredPushObservable<T> : PushSubject<T>
    {
        private Action<Action<T>, CancellationToken> _valuesFactory;
        private WaitHandle _startSynchronizer = null;
        public EventDeferredPushObservable(Action<Action<T>, CancellationToken> valuesFactory, WaitHandle startSynchronizer, CancellationToken cancellationToken) : base(cancellationToken)
        {
            _valuesFactory = valuesFactory;
            _startSynchronizer = startSynchronizer;
            if (startSynchronizer != null)
                this.Start();
        }

        private void Start()
        {
            Task.Run(() =>
            {
                this._startSynchronizer.WaitOne();
                try
                {
                    _valuesFactory(PushValue, base.CancellationToken);
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
