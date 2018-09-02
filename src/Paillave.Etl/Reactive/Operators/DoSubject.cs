using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class DoSubject<T> : PushSubject<T>
    {
        private IDisposable _subscription;
        private object _syncValue = new object();

        public DoSubject(IPushObservable<T> observable, Action<T> action)
        {
            this._subscription = observable.Subscribe(i =>
            {
                lock (_syncValue)
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
                }
            }, this.Complete, this.PushException);
        }

        public override void Dispose()
        {
            lock (_syncValue)
            {
                base.Dispose();
                _subscription.Dispose();
            }
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
