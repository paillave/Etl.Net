using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class MultiMapSubject<TIn, TOut> : PushSubject<TOut>
    {
        private IDisposable _subscription;
        private object _syncLock = new object();
        public MultiMapSubject(IPushObservable<TIn> observable, Action<TIn, Action<TOut>> outputValuesFactory) : base(observable.CancellationToken)
        {
            this._subscription = observable.Subscribe(i =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                lock (_syncLock)
                {
                    try
                    {
                        outputValuesFactory(i, this.PushValue);
                    }
                    catch (Exception ex)
                    {
                        this.PushException(ex);
                    }
                }
            }, this.Complete, this.PushException);
        }
        public MultiMapSubject(IPushObservable<TIn> observable, Action<TIn, int, Action<TOut>> outputValuesFactory) : base(observable.CancellationToken)
        {
            int counter = 0;
            this._subscription = observable.Subscribe(i =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                lock (_syncLock)
                {
                    try
                    {
                        outputValuesFactory(i, ++counter, this.PushValue);
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
        public static IPushObservable<TOut> MultiMap<TIn, TOut>(this IPushObservable<TIn> observable, Action<TIn, Action<TOut>> outputValuesFactory)
        {
            return new MultiMapSubject<TIn, TOut>(observable, outputValuesFactory);
        }
        public static IPushObservable<TOut> MultiMap<TIn, TOut>(this IPushObservable<TIn> observable, Action<TIn, int, Action<TOut>> outputValuesFactory)
        {
            return new MultiMapSubject<TIn, TOut>(observable, outputValuesFactory);
        }
    }
}
