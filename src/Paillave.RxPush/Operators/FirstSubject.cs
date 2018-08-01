using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.RxPush.Operators
{
    public class FirstSubject<T> : PushSubject<T>
    {
        private IDisposable _subscription;
        private object _lockObject = new object();
        private bool _isCompleted = false;
        //private T _lastValue;
        public FirstSubject(IPushObservable<T> observable)
        {
            lock (_lockObject)
            {
                _subscription = observable.Subscribe(HandlePushValue, HandleCompleteValue);
            }
        }

        private void HandleCompleteValue()
        {
            lock (_lockObject)
            {
                if (!_isCompleted)
                    Complete();
            }
        }

        private void HandlePushValue(T value)
        {
            lock (_lockObject)
            {
                if (!_isCompleted)
                {
                    PushValue(value);
                    Complete();
                }
                _isCompleted = true;
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
        public static IPushObservable<TIn> First<TIn>(this IPushObservable<TIn> observable)
        {
            return new FirstSubject<TIn>(observable);
        }
    }
}
