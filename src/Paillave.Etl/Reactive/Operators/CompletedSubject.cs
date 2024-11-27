using Paillave.Etl.Reactive.Core;
using System;

namespace Paillave.Etl.Reactive.Operators
{
    public class CompletedSubject<TIn> : PushSubject<int>
    {
        private IDisposable _subscription;
        private int _count = 0;
        private object _lockSync = new object();
        public CompletedSubject(IPushObservable<TIn> observable) : base(observable.CancellationToken)
        {
            _subscription = observable.Subscribe(i =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                lock (_lockSync)
                {
                    _count++;
                }
            }, this.HandleComplete, this.PushException);
        }

        private void HandleComplete()
        {
            lock (_lockSync)
            {
                PushValue(_count);
                base.Complete();
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
        public static IPushObservable<int> Completed<TIn>(this IPushObservable<TIn> observable) => new CompletedSubject<TIn>(observable);
    }
}
