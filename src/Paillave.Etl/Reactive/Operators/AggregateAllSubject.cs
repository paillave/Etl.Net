using Paillave.Etl.Reactive.Core;
using System;

namespace Paillave.Etl.Reactive.Operators
{
    public class AggregateAllSubject<TIn, TOut> : PushSubject<TOut>
    {
        private IDisposable _subscription;
        private bool _isAggrDisposable = false;
        private bool _hasValue = false;
        private TOut _aggregation = default;
        private object _lockSync = new object();
        public AggregateAllSubject(IPushObservable<TIn> observable, Func<TOut, TIn, TOut> reducer, Func<TIn, TOut> createInitialValue) : base(observable.CancellationToken)
        {
            _isAggrDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TOut));
            _subscription = observable.Subscribe(i =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                lock (_lockSync)
                {

                    if (!_hasValue && createInitialValue != null)
                    {
                        _aggregation = createInitialValue(i);
                        _hasValue = true;
                    }
                    _aggregation = reducer(_aggregation, i);
                }
            }, this.HandleComplete, this.PushException);
        }

        private void HandleComplete()
        {
            lock (_lockSync)
            {
                PushValue(_aggregation);
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
        public static IPushObservable<TOut> Aggregate<TIn, TOut>(this IPushObservable<TIn> observable, Func<TIn, TOut> createInitialAggregation, Func<TOut, TIn, TOut> reducer) => 
            new AggregateAllSubject<TIn, TOut>(observable, reducer, createInitialAggregation);
        public static IPushObservable<TOut> Aggregate<TIn, TOut>(this IPushObservable<TIn> observable, Func<TOut, TIn, TOut> reducer) =>
            new AggregateAllSubject<TIn, TOut>(observable, reducer, null);
    }
}
