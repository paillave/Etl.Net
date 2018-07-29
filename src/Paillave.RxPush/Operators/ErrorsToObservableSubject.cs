using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.RxPush.Operators
{
    public class ErrorsToObservableSubject<T> : PushSubject<Exception>
    {
        private readonly IDisposable _subscription;

        public ErrorsToObservableSubject(IPushObservable<T> observable)
        {
            _subscription = observable.Subscribe(_ => { }, HandleComplete, HandlePushError);
        }

        private void HandleComplete()
        {
            Complete();
        }

        private void HandlePushError(Exception obj)
        {
            PushValue(obj);
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<Exception> ErrorsToObservable<T>(this IPushObservable<T> observable)
        {
            return new ErrorsToObservableSubject<T>(observable);
        }
    }
}
