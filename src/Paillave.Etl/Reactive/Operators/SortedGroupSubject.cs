using System;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class SortedGroupSubjectSubject<TIn, TKey, TOut> : PushSubject<TOut>
    {
        private class KeySortedGroupSubject
        {
            public TKey Key { get; set; }
            public TIn FirstElement { get; set; }
            public IPushSubject<TIn> PushSubject { get; set; }
            public IDisposable OutputSubscription { get; set; }
        }
        private KeySortedGroupSubject _currentGroup = null;
        private readonly Func<TIn, TKey> _getKey;
        private readonly Func<IPushObservable<TIn>, TIn, IPushObservable<TOut>> _groupedObservableTransformation;
        private IDisposable _sourceSubscription;
        // private IDisposableManager _outSubscriptions = new CollectionDisposableManager();
        // private Dictionary<TKey, KeySortedGroupSubject> _observableDictionary = new Dictionary<TKey, KeySortedGroupSubject>();
        private object _syncLock = new object();
        public SortedGroupSubjectSubject(
            IPushObservable<TIn> sourceS,
            Func<TIn, TKey> getKey,
            Func<IPushObservable<TIn>, TIn, IPushObservable<TOut>> groupedObservableTransformation) : base(sourceS.CancellationToken)
        {
            _getKey = getKey;
            _groupedObservableTransformation = groupedObservableTransformation;
            _sourceSubscription = sourceS.Subscribe(OnSourcePush, OnSourceComplete, OnSourceException);
        }
        private void OnSourcePush(TIn value)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_syncLock)
            {
                TKey key = _getKey(value);
                if (_currentGroup == null || !_currentGroup.Key.Equals(key))
                {
                    if (_currentGroup != null)
                    {
                        _currentGroup.PushSubject.Complete();
                        _currentGroup.PushSubject.Dispose();
                    }
                    _currentGroup = new KeySortedGroupSubject
                    {
                        Key = key,
                        FirstElement = value,
                        PushSubject = new PushSubject<TIn>(base.CancellationToken)
                    };
                    _currentGroup.OutputSubscription = _groupedObservableTransformation(_currentGroup.PushSubject, value).Subscribe(
                        i => OnOutputPushValue(_currentGroup, i),
                        () => OnOutputCompleted(_currentGroup),
                        e => OnOutputException(_currentGroup, e)
                    );
                }
                _currentGroup.PushSubject.PushValue(value);
            }
        }
        private void OnOutputPushValue(KeySortedGroupSubject grp, TOut value)
        {
            lock (_syncLock)
            {
                base.PushValue(value);
            }
        }
        private void OnOutputException(KeySortedGroupSubject grp, Exception exception)
        {
            lock (_syncLock)
            {
                base.PushException(exception);
            }
        }
        private void TryComplete()
        {
            if (_sourceSubscription == null && _currentGroup == null)
                base.Complete();
        }
        private void OnOutputCompleted(KeySortedGroupSubject grp)
        {
            lock (_syncLock)
            {
                _currentGroup?.OutputSubscription?.Dispose();
                _currentGroup = null;
                TryComplete();
            }
        }
        private void OnSourceComplete()
        {
            lock (_syncLock)
            {
                _sourceSubscription?.Dispose();
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
                _currentGroup.OutputSubscription.Dispose();
            }
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> SortedGroup<TIn, TKey, TOut>(this IPushObservable<TIn> sourceS, Func<TIn, TKey> getKey, Func<IPushObservable<TIn>, TIn, IPushObservable<TOut>> parallelProcess)
        {
            return new SortedGroupSubjectSubject<TIn, TKey, TOut>(sourceS, getKey, parallelProcess);
        }
    }
}
