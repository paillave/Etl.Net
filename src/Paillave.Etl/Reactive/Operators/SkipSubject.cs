using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class SkipSubject<T> : PushSubject<T>
    {
        private IDisposable _subscription;
        private object _lockObject = new object();

        public SkipSubject(IPushObservable<T> observable, int count) : base(observable.CancellationToken)
        {
            this._subscription = observable.Subscribe(i =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                lock (_lockObject)
                {
                    if (count <= 0)
                        this.PushValue(i);
                    else
                        count--;
                }
            }, this.Complete, (ex) =>
            {
                lock (_lockObject)
                {
                    if (count <= 0) this.PushException(ex);
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
        public static IPushObservable<T> Skip<T>(this IPushObservable<T> observable, int count)
        {
            return new SkipSubject<T>(observable, count);
        }
    }
}
