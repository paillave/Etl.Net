using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class TakeSubject<T> : PushSubject<T>
    {
        private IDisposable _subscription;
        private object _lockObject = new object();

        public TakeSubject(IPushObservable<T> observable, int count)
        {
            this._subscription = observable.Subscribe(i =>
            {
                lock (_lockObject)
                {
                    count--;
                    if (count >= 0) this.PushValue(i);
                    if (count == 0) this.Complete();
                }
            }, this.Complete, (ex) =>
            {
                lock (_lockObject)
                {
                    if (count >= 0) this.PushException(ex);
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
        public static IPushObservable<T> Take<T>(this IPushObservable<T> observable, int count)
        {
            return new TakeSubject<T>(observable, count);
        }
    }
}
