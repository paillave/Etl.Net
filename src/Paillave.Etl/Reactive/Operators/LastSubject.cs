using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Operators
{
    public class LastSubject<T> : PushSubject<T>
    {
        private IDisposable _subscription;
        private object _lockObject = new object();
        private bool _hasLastValue = false;
        private T _lastValue;
        public LastSubject(IPushObservable<T> observable) : base(observable.CancellationToken)
        {
            lock (_lockObject)
            {
                _subscription = observable.Subscribe(HandlePushValue, HandleCompleteValue);
            }
        }

        private void HandleCompleteValue()
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_lockObject)
            {
                if (_hasLastValue) PushValue(_lastValue);
                Complete();
            }
        }

        private void HandlePushValue(T value)
        {
            lock (_lockObject)
            {
                _hasLastValue = true;
                _lastValue = value;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TIn> Last<TIn>(this IPushObservable<TIn> observable) => new LastSubject<TIn>(observable);
    }
}
