using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class SubstractSubject<TLeft, TRight, TKey> : PushSubject<TLeft>
    {
        private object _lockObject = new object();

        private LeftSubscriptionItem<TLeft> _leftSubscriptionItem;
        private RightSubscriptionItem<TRight> _rightSubscriptionItem;
        private IComparer<TLeft, TRight> _comparer;
        private abstract class SubscriptionItemBase<T>
        {
            public IDisposable Subscription { get; set; }
            public IPushObservable<T> Observable { get; set; }
            public bool IsComplete { get; set; } = false;
        }
        private class LeftSubscriptionItem<T> : SubscriptionItemBase<T>
        {
            public Queue<T> Values { get; } = new Queue<T>();
        }
        private class RightSubscriptionItem<T> : SubscriptionItemBase<T>
        {
            public bool HasLastValue { get; private set; } = false;
            private T _lastValue;
            public T LastValue
            {
                get
                {
                    if (!HasLastValue) throw new Exception();
                    return _lastValue;
                }
                set
                {
                    _lastValue = value;
                    HasLastValue = true;
                }
            }
        }
        public SubstractSubject(IPushObservable<TLeft> observable, IPushObservable<TRight> observableToRemove, IComparer<TLeft, TRight> comparer)
        {
            _comparer = comparer;
            _leftSubscriptionItem = new LeftSubscriptionItem<TLeft>
            {
                Observable = observable,
                Subscription = observable.Subscribe(HandlePushValueLeft, HandleCompleteLeft, PushException)
            };
            _rightSubscriptionItem = new RightSubscriptionItem<TRight>
            {
                Observable = observableToRemove,
                Subscription = observableToRemove.Subscribe(HandlePushValueRight, HandleCompleteRight, PushException)
            };
        }
        private void HandlePushValueLeft(TLeft value)
        {
            lock (_lockObject)
            {
                if (!_rightSubscriptionItem.HasLastValue)
                    _leftSubscriptionItem.Values.Enqueue(value);
                else
                {
                    int comp = _comparer.Compare(value, _rightSubscriptionItem.LastValue);
                    if (comp < 0) PushValue(value);
                    else if (comp > 0)
                    {
                        if (_rightSubscriptionItem.IsComplete)
                            PushValue(value);
                        else
                            _leftSubscriptionItem.Values.Enqueue(value);
                    }
                }
            }
        }

        private void HandlePushValueRight(TRight value)
        {
            lock (_lockObject)
            {
                _rightSubscriptionItem.LastValue = value;
                int comp;
                while (_leftSubscriptionItem.Values.Count > 0 && (comp = _comparer.Compare(_leftSubscriptionItem.Values.Peek(), value)) <= 0)
                {
                    var leftValue = _leftSubscriptionItem.Values.Dequeue();
                    if (comp < 0) PushValue(leftValue);
                }
            }
        }

        private void HandleCompleteLeft()
        {
            lock (_lockObject)
            {
                _leftSubscriptionItem.IsComplete = true;
                TryComplete(true);
            }
        }
        private void TryComplete(bool complete)
        {
            if (!_rightSubscriptionItem.IsComplete || !_leftSubscriptionItem.IsComplete) return;
            int comp;
            while (_leftSubscriptionItem.Values.Count > 0)
            {
                var leftValue = _leftSubscriptionItem.Values.Dequeue();
                comp = _comparer.Compare(leftValue, _rightSubscriptionItem.LastValue);
                if (comp != 0) PushValue(leftValue);
            }
            if (complete) base.Complete();
        }
        private void HandleCompleteRight()
        {
            lock (_lockObject)
            {
                _rightSubscriptionItem.IsComplete = true;
                TryComplete(false);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _leftSubscriptionItem.Subscription.Dispose();
            _rightSubscriptionItem.Subscription.Dispose();
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
