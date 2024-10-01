using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.Reactive.Operators
{
    public class AggregateSubject<TIn, TAggr, TKey, TOut> : PushSubject<TOut>
    {
        private CollectionDisposableManager _disposable = new CollectionDisposableManager();
        private readonly Func<TIn, TKey, TAggr, TOut> _resultSelector;
        private IDisposable _subscription;
        private bool _isAggrDisposable = false;
        private Dictionary<TKey, DicoAggregateElement<TIn, TAggr>> _dictionary = new Dictionary<TKey, DicoAggregateElement<TIn, TAggr>>();
        private object _lockSync = new object();
        public AggregateSubject(IPushObservable<TIn> observable, Func<TAggr, TIn, TAggr> reducer, Func<TIn, TKey> getKey, Func<TIn, TAggr> createInitialValue, Func<TIn, TKey, TAggr, TOut> resultSelector) : base(observable.CancellationToken)
        {
            _resultSelector = resultSelector;
            _isAggrDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TAggr));
            _subscription = observable.Subscribe(i =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                lock (_lockSync)
                {
                    TKey key = getKey(i);
                    if (key != null)
                    {
                        if (!_dictionary.TryGetValue(key, out DicoAggregateElement<TIn, TAggr> aggr))
                        {
                            aggr = new DicoAggregateElement<TIn, TAggr> { CurrentAggregation = createInitialValue(i), InValue = i };
                            //aggr.CurrentAggregation = createInitialValue(i);
                            _dictionary[key] = aggr;
                            if (_isAggrDisposable) _disposable.AddRange(aggr as IDisposable);
                        }
                        aggr.CurrentAggregation = reducer(aggr.CurrentAggregation, i);
                        _dictionary[key] = aggr;
                    }
                }
            }, this.HandleComplete, this.PushException);
        }

        private void HandleComplete()
        {
            lock (_lockSync)
            {
                foreach (var item in _dictionary)
                    TryPushValue(() => _resultSelector(item.Value.InValue, item.Key, item.Value.CurrentAggregation));
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
    public class DicoAggregateElement<TIn, TAggr>
    {
        public TIn InValue { get; set; }
        public TAggr CurrentAggregation { get; set; }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> Aggregate<TIn, TAggr, TKey, TOut>(this IPushObservable<TIn> observable, Func<TIn, TAggr> createInitialAggregation, Func<TIn, TKey> getKey, Func<TAggr, TIn, TAggr> reducer, Func<TIn, TKey, TAggr, TOut> resultSelector) => new AggregateSubject<TIn, TAggr, TKey, TOut>(observable, reducer, getKey, createInitialAggregation, resultSelector);
    }
}
