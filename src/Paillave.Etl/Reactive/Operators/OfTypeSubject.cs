using System;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class OfTypeSubject<TIn, TOut> : PushSubject<TOut> where TOut : TIn
    {
        private IDisposable _subscription;

        public OfTypeSubject(IPushObservable<TIn> observable) : base(observable.CancellationToken)
        {
            this._subscription = observable.Subscribe(i =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    if (i is TOut output) this.PushValue(output);
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
        public static IPushObservable<TOut> OfType<TIn, TOut>(this IPushObservable<TIn> observable) where TOut : TIn => new OfTypeSubject<TIn, TOut>(observable);
    }
}
