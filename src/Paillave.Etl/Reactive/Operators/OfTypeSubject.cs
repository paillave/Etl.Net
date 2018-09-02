using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class OfTypeSubject<TIn, TOut> : PushSubject<TOut> where TOut : class, TIn
    {
        private IDisposable _subscription;

        public OfTypeSubject(IPushObservable<TIn> observable)
        {
            this._subscription = observable.Subscribe(i =>
            {
                try
                {
                    TOut ret = i as TOut;
                    if (ret != null) this.PushValue(ret);
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
        public static IPushObservable<TOut> OfType<TOut>(this IPushObservable<object> observable) where TOut : class
        {
            return new OfTypeSubject<object, TOut>(observable);
        }
    }
}
