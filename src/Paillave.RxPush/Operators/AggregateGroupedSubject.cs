using Paillave.RxPush.Core;
using Paillave.RxPush.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.RxPush.Operators
{
    public class AggregateGroupedSubject<TIn, TAggr, TKey> : PushSubject<KeyValuePair<TKey, TAggr>>
    {
        private SingleDisposableManager _disposable = new SingleDisposableManager();
        private IDisposable _subscription;
        private bool _hasValue = false;
        private KeyValuePair<TKey, TAggr> _currentAggregation = new KeyValuePair<TKey, TAggr>();
        private object _lockSync = new object();
        public AggregateGroupedSubject(IPushObservable<TIn> observable, Func<TAggr, TIn, TAggr> reducer, Func<TIn, TKey> getKey, Func<TAggr> createInitialValue)
        {
            var _isAggrDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TAggr));
            _subscription = observable.Subscribe(i =>
            {
                lock (_lockSync)
                {
                    TKey key = getKey(i);
                    if (_hasValue && !key.Equals(_currentAggregation.Key))
                        PushValue(_currentAggregation);
                    if (!_hasValue || !key.Equals(_currentAggregation.Key))
                    {
                        var aggr = createInitialValue();
                        if (_isAggrDisposable) _disposable.Set(aggr as IDisposable);
                        _currentAggregation = new KeyValuePair<TKey, TAggr>(key, aggr);
                        _hasValue = true;
                    }
                    _currentAggregation = new KeyValuePair<TKey, TAggr>(key, reducer(_currentAggregation.Value, i));
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
        public static IPushObservable<KeyValuePair<TKey, TAggr>> AggregateGrouped<TIn, TAggr, TKey>(this IPushObservable<TIn> observable, Func<TAggr> createInitialValue, Func<TIn, TKey> getKey, Func<TAggr, TIn, TAggr> reducer)
        {
            return new AggregateGroupedSubject<TIn, TAggr, TKey>(observable, reducer, getKey, createInitialValue);
        }
    }
}
