using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.Reactive.Operators
{
    public class AggregateGroupedSubject<TIn, TAggr, TKey, TOut> : PushSubject<TOut>
    {
        private SingleDisposableManager _disposable = new SingleDisposableManager();
        private IDisposable _subscription;
        private bool _hasValue = false;
        private KeyValuePair<TKey, DicoAggregateElement<TIn, TAggr>> _currentAggregation = new KeyValuePair<TKey, DicoAggregateElement<TIn, TAggr>>();
        private object _lockSync = new object();
        private Func<TIn, TKey, TAggr, TOut> _resultSelector;

        public AggregateGroupedSubject(IPushObservable<TIn> observable, Func<TAggr, TIn, TAggr> reducer, Func<TIn, TKey> getKey, Func<TIn, TAggr> createInitialValue, Func<TIn, TKey, TAggr, TOut> resultSelector)
        {
            _resultSelector = resultSelector;
            var _isAggrDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TAggr));
            _subscription = observable.Subscribe(i =>
            {
                lock (_lockSync)
                {
                    TKey key = getKey(i);
                    if (key != null)
                    {
                        if (_hasValue && !key.Equals(_currentAggregation.Key))
                            PushValue(_resultSelector(_currentAggregation.Value.InValue, _currentAggregation.Key, _currentAggregation.Value.CurrentAggregation));
                        if (!_hasValue || !key.Equals(_currentAggregation.Key))
                        {
                            var aggr = createInitialValue(i);
                            if (_isAggrDisposable) _disposable.Set(aggr as IDisposable);
                            _currentAggregation = new KeyValuePair<TKey, DicoAggregateElement<TIn, TAggr>>(key, new DicoAggregateElement<TIn, TAggr> { CurrentAggregation = aggr, InValue = i });
                            _hasValue = true;
                        }
                        _currentAggregation.Value.CurrentAggregation = reducer(_currentAggregation.Value.CurrentAggregation, i);
                    }
                }
            }, this.HandleComplete, this.PushException);
        }
        private void HandleComplete()
        {
            lock (_lockSync)
            {
                if (_hasValue)
                    PushValue(_resultSelector(_currentAggregation.Value.InValue, _currentAggregation.Key, _currentAggregation.Value.CurrentAggregation));
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
        public static IPushObservable<TOut> AggregateGrouped<TIn, TAggr, TKey, TOut>(this IPushObservable<TIn> observable, Func<TIn, TAggr> createInitialValue, Func<TIn, TKey> getKey, Func<TAggr, TIn, TAggr> reducer, Func<TIn, TKey, TAggr, TOut> resultSelector)
        {
            return new AggregateGroupedSubject<TIn, TAggr, TKey, TOut>(observable, reducer, getKey, createInitialValue, resultSelector);
        }
    }
}
