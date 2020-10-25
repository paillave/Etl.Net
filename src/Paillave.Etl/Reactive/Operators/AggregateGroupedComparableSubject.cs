using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.Reactive.Operators
{
    public class AggregateGroupedComparableSubject<TIn, TAggr, TOut> : PushSubject<TOut>
    {
        private SingleDisposableManager _disposable = new SingleDisposableManager();
        private IDisposable _subscription;
        private bool _hasValue = false;
        private DicoAggregateElement<TIn, TAggr> _currentAggregation = new DicoAggregateElement<TIn, TAggr>();
        private object _lockSync = new object();
        private Func<TIn, TAggr, TOut> _resultSelector;
        public AggregateGroupedComparableSubject(IPushObservable<TIn> observable, Func<TAggr, TIn, TAggr> reducer, IEqualityComparer<TIn> equalityComparer, Func<TIn, TAggr> createInitialValue, Func<TIn, TAggr, TOut> resultSelector) : base(observable.CancellationToken)
        {
            _resultSelector = resultSelector;
            var _isAggrDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TAggr));
            _subscription = observable.Subscribe(i =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                lock (_lockSync)
                {
                    if (_hasValue && !equalityComparer.Equals(_currentAggregation.InValue, i))
                        TryPushValue(() => _resultSelector(_currentAggregation.InValue, _currentAggregation.CurrentAggregation));
                    if (!_hasValue || !equalityComparer.Equals(_currentAggregation.InValue, i))
                    {
                        var aggr = createInitialValue(i);
                        if (_isAggrDisposable) _disposable.Set(aggr as IDisposable);
                        _currentAggregation = new DicoAggregateElement<TIn, TAggr> { InValue = i, CurrentAggregation = aggr };
                        _hasValue = true;
                    }
                    _currentAggregation.CurrentAggregation = reducer(_currentAggregation.CurrentAggregation, i);
                }
            }, this.HandleComplete, this.PushException);
        }
        private void HandleComplete()
        {
            lock (_lockSync)
            {
                if (_hasValue)
                    TryPushValue(() => _resultSelector(_currentAggregation.InValue, _currentAggregation.CurrentAggregation));
                _disposable.Dispose();
                base.Complete();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _disposable.Dispose();
            _subscription.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> AggregateGrouped<TIn, TAggr, TOut>(this IPushObservable<TIn> observable, Func<TIn, TAggr> createInitialValue, IEqualityComparer<TIn> equalityComparer, Func<TAggr, TIn, TAggr> reducer, Func<TIn, TAggr, TOut> resultSelector)
        {
            return new AggregateGroupedComparableSubject<TIn, TAggr, TOut>(observable, reducer, equalityComparer, createInitialValue, resultSelector);
        }
    }
}
