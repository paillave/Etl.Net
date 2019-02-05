using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public static class QueueEx
    {
        public static bool TryPeek<T>(this Queue<T> queue, out T value)
        {
            if (queue.Count == 0)
            {
                value = default(T);
                return false;
            }
            value = queue.Peek();
            return true;
        }
        public static bool IsEmpty<T>(this Queue<T> queue) => queue.Count == 0;
        public static bool IsLastEntry<T>(this Queue<T> queue) => queue.Count == 1;

        public static bool TryDequeue<T>(this Queue<T> queue, out T value)
        {
            if (queue.Count == 0)
            {
                value = default(T);
                return false;
            }
            value = queue.Dequeue();
            return true;
        }
    }
    public class SubstractSubject<TLeft, TRight, TKey> : PushSubject<TLeft>
    {
        private object _lockObject = new object();

        private SubscriptionItem<TLeft> _leftSubscriptionItem;
        private SubscriptionItem<TRight> _rightSubscriptionItem;
        private IComparer<TLeft, TRight> _comparer;
        private class SubscriptionItem<T> : IDisposable
        {
            private T _lastValue;
            public T LastValue => _lastValue;
            public void SetLastValue(T lastValue)
            {
                _lastValue = lastValue;
                HasLastValue = true;
            }
            public bool HasLastValue { get; private set; } = false;
            private IDisposable _subscription;
            private IPushObservable<T> _observable;
            public bool IsComplete { get; set; } = false;
            public Queue<T> Values { get; } = new Queue<T>();
            public SubscriptionItem(IPushObservable<T> observable, Action<T> onPushValue, Action onComplete, Action<Exception> onException)
            {
                this._observable = observable;
                _subscription = observable.Subscribe(onPushValue, onComplete, onException);
            }
            public void Dispose() => this._subscription.Dispose();
        }
        public SubstractSubject(IPushObservable<TLeft> observable, IPushObservable<TRight> observableToRemove, IComparer<TLeft, TRight> comparer)
        {
            _comparer = comparer;
            _leftSubscriptionItem = new SubscriptionItem<TLeft>(observable, HandlePushValueLeft, HandleCompleteLeft, PushException);
            _rightSubscriptionItem = new SubscriptionItem<TRight>(observableToRemove, HandlePushValueRight, HandleCompleteRight, PushException);
        }
        private void HandlePushValueLeft(TLeft value)
        {
            lock (_lockObject)
            {
                _leftSubscriptionItem.SetLastValue(value);
                int? comp = null;
                foreach (var rightItem in _rightSubscriptionItem.Values)
                {
                    comp = _comparer.Compare(value, rightItem);
                    if (comp == 0) break;
                }
                if (comp == null)
                    _leftSubscriptionItem.Values.Enqueue(value);
                else if (comp < 0 || (comp > 0 && _rightSubscriptionItem.IsComplete))
                    PushValue(value);
                else if (comp > 0)
                    _leftSubscriptionItem.Values.Enqueue(value);
            }
        }

        private void HandlePushValueRight(TRight value)
        {
            lock (_lockObject)
            {
                _rightSubscriptionItem.SetLastValue(value);
                int comp;

                if (_leftSubscriptionItem.HasLastValue)
                {
                    var greaterLeft = _leftSubscriptionItem.LastValue;
                    while (_rightSubscriptionItem.Values.TryPeek(out var rightValue) && (comp = _comparer.Compare(greaterLeft, rightValue)) > 0)
                    {
                        _rightSubscriptionItem.Values.Dequeue();
                    }
                }
                _rightSubscriptionItem.Values.Enqueue(value);
                while (_leftSubscriptionItem.Values.TryPeek(out var leftValue) && (comp = _comparer.Compare(leftValue, value)) <= 0)
                {
                    _leftSubscriptionItem.Values.Dequeue();
                    if (comp < 0) PushValue(leftValue);
                }
            }
        }

        private void HandleCompleteLeft()
        {
            lock (_lockObject)
            {
                _leftSubscriptionItem.IsComplete = true;
                TryComplete();
            }
        }
        private void TryComplete()
        {
            if (!_rightSubscriptionItem.IsComplete || !_leftSubscriptionItem.IsComplete) return;
            int comp;
            bool hasRightValue = _rightSubscriptionItem.Values.TryPeek(out TRight rightValue);
            while (_leftSubscriptionItem.Values.TryDequeue(out var leftValue))
            {
                if (!hasRightValue)
                {
                    PushValue(leftValue);
                }
                else
                {
                    comp = _comparer.Compare(leftValue, rightValue);
                    while (comp > 0 && (hasRightValue = _rightSubscriptionItem.Values.TryDequeue(out rightValue)))
                    {
                        comp = _comparer.Compare(leftValue, rightValue);
                    }
                    if (comp < 0 || (comp > 0 && _rightSubscriptionItem.Values.IsEmpty())) PushValue(leftValue);
                }
            }
            base.Complete();
        }
        private void HandleCompleteRight()
        {
            lock (_lockObject)
            {
                _rightSubscriptionItem.IsComplete = true;
                TryComplete();
            }
        }
        public override void Dispose()
        {
            lock (_lockObject)
            {
                base.Dispose();
                _leftSubscriptionItem.Dispose();
                _rightSubscriptionItem.Dispose();
            }
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TLeft> Substract<TLeft, TRight, TKey>(this IPushObservable<TLeft> observable, IPushObservable<TRight> observableToRemove, IComparer<TLeft, TRight> comparer)
        {
            return new SubstractSubject<TLeft, TRight, TKey>(observable, observableToRemove, comparer);
        }
        public static IPushObservable<TLeft> Substract<TLeft, TRight, TKey>(this IPushObservable<TLeft> observable, IPushObservable<TRight> observableToRemove, Func<TLeft, TKey> getLeftKey, Func<TRight, TKey> getRightKey, object keyPosition = null)
        {
            return new SubstractSubject<TLeft, TRight, TKey>(observable, observableToRemove, new SortDefinitionComparer<TLeft, TRight, TKey>(SortDefinition.Create(getLeftKey, keyPosition), SortDefinition.Create(getRightKey, keyPosition)));
        }
    }
}
