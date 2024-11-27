using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class ScanSubject<TIn, TOut> : PushSubject<TOut>
    {
        private IDisposable _subscription;
        private object _lockSync = new object();
        public ScanSubject(IPushObservable<TIn> observable, Func<TOut, TIn, TOut> reducer, TOut initialValue) : base(observable.CancellationToken)
        {
            this._subscription = observable.Subscribe(i =>
            {
                lock (_lockSync)
                {
                    try
                    {
                        initialValue = reducer(initialValue, i);
                        this.PushValue(initialValue);
                    }
                    catch (Exception ex)
                    {
                        this.PushException(ex);
                    }
                }
            }, this.Complete, this.PushException);
        }
        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> Scan<TIn, TOut>(this IPushObservable<TIn> observable, TOut initialValue, Func<TOut, TIn, TOut> reducer) => new ScanSubject<TIn, TOut>(observable, reducer, initialValue);
        public static IPushObservable<TOut> Scan<TIn, TOut>(this IPushObservable<TIn> observable, Func<TOut, TIn, TOut> reducer) => new ScanSubject<TIn, TOut>(observable, reducer, default(TOut));
    }
}
