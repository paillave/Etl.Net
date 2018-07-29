using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;

namespace Paillave.RxPush.Operators
{
    public class DoSubject<T> : PushSubject<T>
    {
        private IDisposable _subscription;

        public DoSubject(IPushObservable<T> observable, Action<T> action)
        {
            this._subscription = observable.Subscribe(i =>
            {
                try
                {
                    action(i);
                }
                catch (Exception ex)
                {
                    this.PushException(ex);
                }
                this.PushValue(i);
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
        public static IPushObservable<T> Do<T>(this IPushObservable<T> observable, Action<T> action)
        {
            return new DoSubject<T>(observable, action);
        }
    }
}
