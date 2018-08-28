using Paillave.RxPush.Core;
using Paillave.RxPush.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.RxPush.Operators
{
    public class AggregateGroupedSubject<TIn, TAggr, TKey> : PushSubject<KeyValuePair<TKey, TAggr>> where TKey : IEquatable<TKey>
    {
        private CollectionDisposableManager _disposable = new CollectionDisposableManager();
        private IDisposable _subscription;
        private Dictionary<TKey, TAggr> _dictionary = new Dictionary<TKey, TAggr>();
        private object _lockSync = new object();
        public AggregateGroupedSubject(IPushObservable<TIn> observable, Action<TAggr, TIn> reducer, Func<TIn, TKey> getKey, Func<TAggr> createInitialValue)
        {
            //TODO
            throw new NotImplementedException();
            var _isAggrDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TAggr));
            _subscription = observable.Subscribe(i =>
            {
                lock (_lockSync)
                {
                    TKey key = getKey(i);
                    if (_dictionary.TryGetValue(key, out TAggr aggr))
                    {
                        aggr = createInitialValue();
                        _dictionary[key] = aggr;
                        if (_isAggrDisposable) _disposable.Set(aggr as IDisposable);
                    }
                    reducer(aggr, i);
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
            _subscription.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<KeyValuePair<TKey, TAggr>> AggregateGrouped<TIn, TAggr, TKey>(this IPushObservable<TIn> observable, Action<TAggr, TIn> reducer, Func<TIn, TKey> getKey, Func<TAggr> createInitialValue) where TKey : IEquatable<TKey>
        {
            return new AggregateGroupedSubject<TIn, TAggr, TKey>(observable, reducer, getKey, createInitialValue);
        }
    }
}
