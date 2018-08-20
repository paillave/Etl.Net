using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;

namespace Paillave.RxPush.Operators
{
    public class ToListSubject<TIn> : PushSubject<List<TIn>>
    {
        private IDisposable _subscription;
        private List<TIn> _accumulator = new List<TIn>();
        private object _lockSync = new object();
        public ToListSubject(IPushObservable<TIn> observable)
        {
            this._subscription = observable.Subscribe(this.HandlePushValue, this.HandleComplete, this.PushException);
        }
        private void HandlePushValue(TIn value)
        {
            lock (_lockSync)
            {
                _accumulator.Add(value);
            }
        }
        private void HandleComplete()
        {
            lock (_lockSync)
            {
                this.PushValue(_accumulator);
                this.Complete();
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
        public static IPushObservable<List<TIn>> ToList<TIn>(this IPushObservable<TIn> observable)
        {
            return new ToListSubject<TIn>(observable);
        }
    }
}
