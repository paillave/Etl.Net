using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class MapSubject<TIn, TOut> : PushSubject<TOut>
    {
        private IDisposable _subscription;

        public MapSubject(IPushObservable<TIn> observable, Func<TIn, TOut> selector)
        {
            this._subscription = observable.Subscribe(i =>
            {
                try
                {
                    this.PushValue(selector(i));
                }
                catch (Exception ex)
                {
                    this.PushException(ex);
                }
            }, this.Complete, this.PushException);
        }
        public MapSubject(IPushObservable<TIn> observable, Func<TIn, int, TOut> selector)
        {
            int counter = 0;
            this._subscription = observable.Subscribe(i =>
            {
                try
                {
                    this.PushValue(selector(i, ++counter));
                }
                catch (Exception ex)
                {
                    this.PushException(ex);
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
        public static IPushObservable<TOut> Map<TIn, TOut>(this IPushObservable<TIn> observable, Func<TIn, TOut> selector)
        {
            return new MapSubject<TIn, TOut>(observable, selector);
        }
        public static IPushObservable<TOut> Map<TIn, TOut>(this IPushObservable<TIn> observable, Func<TIn, int, TOut> selector)
        {
            return new MapSubject<TIn, TOut>(observable, selector);
        }
    }
}
