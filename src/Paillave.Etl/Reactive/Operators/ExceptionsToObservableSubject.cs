using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Operators
{
    public class ExceptionsToObservableSubject<T> : PushSubject<Exception>
    {
        private readonly IDisposable _subscription;

        public ExceptionsToObservableSubject(IPushObservable<T> observable) : base(observable.CancellationToken) => _subscription = observable.Subscribe(_ => { }, HandleComplete, HandlePushError);

        private void HandleComplete() => Complete();

        private void HandlePushError(Exception obj)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            PushValue(obj);
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<Exception> ExceptionsToObservable<T>(this IPushObservable<T> observable) => new ExceptionsToObservableSubject<T>(observable);
    }
}
