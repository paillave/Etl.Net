using Paillave.RxPush.Core;
using Paillave.RxPush.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.RxPush.Operators
{
    public class AggregateGroupedComparableSubject<TIn, TAggr> : PushSubject<KeyValuePair<TIn, TAggr>>
    {
        private SingleDisposableManager _disposable = new SingleDisposableManager();
        private IDisposable _subscription;
        private bool _hasValue = false;
        private KeyValuePair<TIn, TAggr> _currentAggregation = new KeyValuePair<TIn, TAggr>();
        private object _lockSync = new object();
        public AggregateGroupedComparableSubject(IPushObservable<TIn> observable, Func<TAggr, TIn, TAggr> reducer, IEqualityComparer<TIn> equalityComparer, Func<TAggr> createInitialValue)
        {
            var _isAggrDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TAggr));
            _subscription = observable.Subscribe(i =>
            {
                lock (_lockSync)
                {
                    if (_hasValue && !equalityComparer.Equals(_currentAggregation.Key, i))
                        PushValue(_currentAggregation);
                    if (!_hasValue || !equalityComparer.Equals(_currentAggregation.Key, i))
                    {
                        var aggr = createInitialValue();
                        if (_isAggrDisposable) _disposable.Set(aggr as IDisposable);
                        _currentAggregation = new KeyValuePair<TIn, TAggr>(i, aggr);
                        _hasValue = true;
                    }
                    _currentAggregation = new KeyValuePair<TIn, TAggr>(_currentAggregation.Key, reducer(_currentAggregation.Value, i));
                }
            }, this.HandleComplete, this.PushException);
        }
        private void HandleComplete()
        {
            lock (_lockSync)
            {
                if (_hasValue)
                    PushValue(_currentAggregation);
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
        public static IPushObservable<KeyValuePair<TIn, TAggr>> AggregateGrouped<TIn, TAggr>(this IPushObservable<TIn> observable, Func<TAggr> createInitialValue, IEqualityComparer<TIn> equalityComparer, Func<TAggr, TIn, TAggr> reducer)
        {
            return new AggregateGroupedComparableSubject<TIn, TAggr>(observable, reducer, equalityComparer, createInitialValue);
        }
    }
}
