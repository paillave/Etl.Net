using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;

namespace Paillave.Etl.Reactive.Operators
{
    public class ParallelizeSubject<TIn, TKey, TOut> : PushSubject<TOut>
    {
        private class KeyGroup
        {
            public TKey Key { get; set; }
            public IPushSubject<TIn> PushSubject { get; set; }
            public IDisposable OutputSubscription { get; set; }
        }
        private readonly Func<TIn, TKey> _getKey;
        private readonly Func<IPushObservable<TIn>, IPushObservable<TOut>> _groupedObservableTransformation;
        private IDisposable _sourceSubscription;
        private IDisposableManager _outSubscriptions = new CollectionDisposableManager();
        private Dictionary<TKey, KeyGroup> _observableDictionary = new Dictionary<TKey, KeyGroup>();
        private object _syncLock = new object();
        public ParallelizeSubject(
            IPushObservable<TIn> sourceS,
            Func<TIn, TKey> getKey,
            Func<IPushObservable<TIn>, IPushObservable<TOut>> groupedObservableTransformation)
        {
            _getKey = getKey;
            _groupedObservableTransformation = groupedObservableTransformation;
            _sourceSubscription = sourceS.Subscribe(OnSourcePush, OnSourceComplete, OnSourceException);
        }
        private KeyGroup GetOrCreateObservable(TIn value)
        {
            TKey key = _getKey(value);
            KeyGroup grp;
            if (!_observableDictionary.TryGetValue(key, out grp))
            {
                grp = new KeyGroup
                {
                    Key = key,
                    PushSubject = new PushSubject<TIn>()
                };
                grp.OutputSubscription = _groupedObservableTransformation(grp.PushSubject).Subscribe(
                        i => OnOutputPushValue(grp, i),
                        () => OnOutputCompleted(grp),
                        e => OnOutputException(grp, e)
                    );
                _outSubscriptions.Set(grp.OutputSubscription);
                _observableDictionary[key] = grp;
            }
            return grp;
        }
        private void OnSourcePush(TIn value)
        {
            lock (_syncLock)
            {
                var obs = GetOrCreateObservable(value);
                obs.PushSubject.PushValue(value);
            }
        }
        private void OnOutputPushValue(KeyGroup grp, TOut value)
        {
            lock (_syncLock)
            {
                base.PushValue(value);
            }
        }
        private void OnOutputException(KeyGroup grp, Exception exception)
        {
            lock (_syncLock)
            {
                base.PushException(exception);
            }
        }
        private void TryComplete()
        {
            if (_observableDictionary.Count == 0)
                base.Complete();
        }
        private void OnOutputCompleted(KeyGroup grp)
        {
            lock (_syncLock)
            {
                _outSubscriptions.TryDispose(grp.OutputSubscription);
                _observableDictionary.Remove(grp.Key);
                TryComplete();
            }
        }
        private void OnSourceComplete()
        {
            lock (_syncLock)
            {
                foreach (var item in _observableDictionary)
                    item.Value.PushSubject.Complete();
            }
        }
        private void OnSourceException(Exception ex)
        {
            base.PushException(ex);
        }
        public override void Dispose()
        {
            lock (_syncLock)
            {
                base.Dispose();
                _sourceSubscription?.Dispose();
                _outSubscriptions?.Dispose();
            }
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> Parallelize<TIn, TKey, TOut>(this IPushObservable<TIn> sourceS, Func<TIn, TKey> getKey, Func<IPushObservable<TIn>, IPushObservable<TOut>> parallelProcess)
        {
            return new ParallelizeSubject<TIn, TKey, TOut>(sourceS, getKey, parallelProcess);
        }
    }
}
