using System;
using System.Collections.Generic;
using System.Linq;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;

namespace Paillave.Etl.Reactive.Operators
{
    public class GroupSubject<TIn, TKey, TOut> : PushSubject<TOut>
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
        public GroupSubject(
            IPushObservable<TIn> sourceS,
            Func<TIn, TKey> getKey,
            Func<IPushObservable<TIn>, IPushObservable<TOut>> groupedObservableTransformation) : base(sourceS.CancellationToken)
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
                    PushSubject = new PushSubject<TIn>(this.CancellationToken)
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
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_syncLock)
            {
                try
                {
                    var obs = GetOrCreateObservable(value);
                    obs.PushSubject.PushValue(value);
                }
                catch (Exception ex)
                {
                    this.PushException(ex);
                }
            }
        }
        private void OnOutputPushValue(KeyGroup grp, TOut value)
        {
            base.PushValue(value);
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
            if (_sourceSubscription == null && _outSubscriptions.IsDisposed)
                base.Complete();
        }
        private void OnOutputCompleted(KeyGroup grp)
        {
            lock (_syncLock)
            {
                _outSubscriptions.TryDispose(grp.OutputSubscription);
                TryComplete();
            }
        }
        private void OnSourceComplete()
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_syncLock)
            {
                foreach (var item in _observableDictionary.ToList())
                {
                    if (CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    item.Value.PushSubject.Complete();
                }
                _sourceSubscription.Dispose();
                _sourceSubscription = null;
                TryComplete();
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
        public static IPushObservable<TOut> Group<TIn, TKey, TOut>(this IPushObservable<TIn> sourceS, Func<TIn, TKey> getKey, Func<IPushObservable<TIn>, IPushObservable<TOut>> parallelProcess)
        {
            return new GroupSubject<TIn, TKey, TOut>(sourceS, getKey, parallelProcess);
        }
    }
}
