using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Operators
{
    public class SkipUntilSubject<TIn, TFrom> : PushSubject<TIn>
    {
        private object _lockObject = new object();
        private IDisposable _disp1;
        private IDisposable _disp2;
        private bool _isTriggered = false;
        public SkipUntilSubject(IPushObservable<TIn> observable, IPushObservable<TFrom> fromObservable) : base(CancellationTokenSource.CreateLinkedTokenSource(observable.CancellationToken, fromObservable.CancellationToken).Token)
        {
            _disp1 = observable.Subscribe(HandleOnPush, HandleOnComplete, HandleOnError);
            _disp2 = fromObservable.Subscribe(HandleOnPushTrigger, HandleOnCompleteTrigger);
        }

        private void HandleOnCompleteTrigger()
        {
            lock (_lockObject)
            {
                if (!_isTriggered) Complete();
            }
        }

        private void HandleOnPushTrigger(TFrom obj)
        {
            lock (_lockObject)
            {
                _isTriggered = true;
            }
        }

        private void HandleOnError(Exception ex)
        {
            lock (_lockObject)
            {
                if (_isTriggered) PushException(ex);
            }
        }

        private void HandleOnComplete()
        {
            lock (_lockObject)
            {
                Complete();
            }
        }

        private void HandleOnPush(TIn value)
        {
            lock (_lockObject)
            {
                if (_isTriggered) PushValue(value);
            }
        }
        private void HandleOnPushCondition(TIn value)
        {
            lock (_lockObject)
            {
                if (_isTriggered) PushValue(value);
            }
        }
        public override void Dispose()
        {
            _disp1.Dispose();
            _disp2.Dispose();
            base.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TIn> SkipUntil<TIn, TFrom>(this IPushObservable<TIn> observable, IPushObservable<TFrom> fromObservable)
        {
            return new SkipUntilSubject<TIn, TFrom>(observable, fromObservable);
        }
    }
}
