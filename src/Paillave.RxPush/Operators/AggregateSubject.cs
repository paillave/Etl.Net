using Paillave.RxPush.Core;
using Paillave.RxPush.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.RxPush.Operators
{
    public class AggregateSubject<TIn, TAggr, TKey> : PushSubject<KeyValuePair<TKey, TAggr>>
    {
        private CollectionDisposableManager _disposable = new CollectionDisposableManager();
        private IDisposable _subscription;
        private bool _isAggrDisposable = false;
        private Dictionary<TKey, TAggr> _dictionary = new Dictionary<TKey, TAggr>();
        private object _lockSync = new object();
        public AggregateSubject(IPushObservable<TIn> observable, Func<TAggr, TIn, TAggr> reducer, Func<TIn, TKey> getKey, Func<TAggr> createInitialValue)
        {
            _isAggrDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TAggr));
            _subscription = observable.Subscribe(i =>
            {
                lock (_lockSync)
                {
                    TKey key = getKey(i);
                    if (!_dictionary.TryGetValue(key, out TAggr aggr))
                    {
                        aggr = createInitialValue();
                        _dictionary[key] = aggr;
                        if (_isAggrDisposable) _disposable.Set(aggr as IDisposable);
                    }
                    aggr = reducer(aggr, i);
                    _dictionary[key] = aggr;
                }
            }, this.HandleComplete, this.PushException);
        }

        private void HandleComplete()
        {
            lock (_lockSync)
            {
                foreach (var item in _dictionary)
                    PushValue(item);
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
        public static IPushObservable<KeyValuePair<TKey, TAggr>> Aggregate<TIn, TAggr, TKey>(this IPushObservable<TIn> observable, Func<TAggr> createInitialValue, Func<TIn, TKey> getKey, Func<TAggr, TIn, TAggr> reducer)
        {
            return new AggregateSubject<TIn, TAggr, TKey>(observable, reducer, getKey, createInitialValue);
        }
    }
}
