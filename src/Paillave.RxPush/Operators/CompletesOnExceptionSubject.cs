using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;

namespace Paillave.RxPush.Operators
{
    public class CompletesOnExceptionSubject<T> : PushSubject<T>
    {
        private IDisposable _subscription;
        private object lockObject = new object();

        public CompletesOnExceptionSubject(IPushObservable<T> observable, Action<Exception> catchMethod)
        {

            this._subscription = observable.Subscribe(this.PushValue, this.Complete, ex =>
            {
                lock (lockObject)
                {
                    catchMethod(ex);
                    this.Complete();
                }
            });
        }
        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<T> CompletesOnException<T>(this IPushObservable<T> observable, Action<Exception> catchMethod)
        {
            return new CompletesOnExceptionSubject<T>(observable, catchMethod);
        }
        public static IPushObservable<T> CompletesOnException<T>(this IPushObservable<T> observable)
        {
            return new CompletesOnExceptionSubject<T>(observable, _ => { });
        }
    }
}
