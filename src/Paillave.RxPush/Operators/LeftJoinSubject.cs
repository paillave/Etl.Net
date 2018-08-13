using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using System.Linq.Expressions;

namespace Paillave.RxPush.Operators
{
    public class LeftJoinSubject<TInLeft, TInRight, TOut> : PushSubject<TOut>
    {
        private IDisposable _leftSubscription;
        private IDisposable _rightSubscription;

        private class Side<T>
        {
            public bool IsComplete { get; set; } = false;
            private Queue<T> _queue = new Queue<T>();
            public bool IsEmpty { get { return _queue.Count == 0; } }
            public T CurrentValue { get { return _queue.Peek(); } }
            public T Dequeue() => _queue.Dequeue();
            public void Enqueue(T element) => _queue.Enqueue(element);
        }

        public LeftJoinSubject(IPushObservable<TInLeft> leftS, IPushObservable<TInRight> rightS, IComparer<TInLeft, TInRight> comparer, Func<TInLeft, TInRight, TOut> selector)
        {
            var leftSide = new Side<TInLeft>();
            var rightSide = new Side<TInRight>();

            object gate = new object();

            Action TryUnstackQueues = () =>
            {
                bool somethingChanged;
                do
                {
                    somethingChanged = false;
                    while (!rightSide.IsEmpty && !leftSide.IsEmpty && comparer.Compare(leftSide.CurrentValue, rightSide.CurrentValue) > 0)
                    {
                        rightSide.Dequeue();
                        somethingChanged = true;
                    }

                    int comparison;
                    while (!leftSide.IsEmpty && !rightSide.IsEmpty && (comparison = comparer.Compare(leftSide.CurrentValue, rightSide.CurrentValue)) <= 0)
                    {
                        TOut ret;
                        try
                        {
                            ret = selector(leftSide.Dequeue(), comparison == 0 ? rightSide.CurrentValue : default(TInRight));
                            this.PushValue(ret);
                        }
                        catch (Exception ex)
                        {
                            PushException(ex);
                        }
                        somethingChanged = true;
                    }

                    if (rightSide.IsEmpty && rightSide.IsComplete)
                        while (!leftSide.IsEmpty)
                        {
                            TOut ret;
                            try
                            {
                                ret = selector(leftSide.Dequeue(), default(TInRight));
                                this.PushValue(ret);
                            }
                            catch (Exception ex)
                            {
                                PushException(ex);
                            }
                            somethingChanged = true;
                        }
                } while (somethingChanged);
                if (leftSide.IsComplete) this.Complete();
            };

            _leftSubscription = leftS.Subscribe(
                    (leftValue) =>
                    {
                        lock (gate)
                        {
                            leftSide.Enqueue(leftValue);
                            TryUnstackQueues();
                        }
                    },
                    () =>
                    {
                        lock (gate)
                        {
                            leftSide.IsComplete = true;
                            TryUnstackQueues();
                        }
                    }
                );
            _rightSubscription = rightS.Subscribe(
                    (rightValue) =>
                    {
                        lock (gate)
                        {
                            rightSide.Enqueue(rightValue);
                            TryUnstackQueues();
                        }
                    },
                    () =>
                    {
                        lock (gate)
                        {
                            rightSide.IsComplete = true;
                            TryUnstackQueues();
                        }
                    }
                );
        }

        public override void Dispose()
        {
            base.Dispose();
            _leftSubscription.Dispose();
            _rightSubscription.Dispose();
        }
    }

    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> LeftJoin<TInLeft, TInRight, TOut>(this IPushObservable<TInLeft> observable, IPushObservable<TInRight> rightS, IComparer<TInLeft, TInRight> comparer, Func<TInLeft, TInRight, TOut> selector)
        {
            return new LeftJoinSubject<TInLeft, TInRight, TOut>(observable, rightS, comparer, selector);
        }
        public static IPushObservable<TOut> LeftJoin<TInLeft, TInRight, TOut>(this IPushObservable<TInLeft> observable, IPushObservable<TInRight> rightS, Expression<Func<TInLeft, IComparable>> leftKey, Expression<Func<TInRight, IComparable>> rightKey, Func<TInLeft, TInRight, TOut> selector)
        {
            return new LeftJoinSubject<TInLeft, TInRight, TOut>(
                observable,
                rightS,
                new SortCriteriaComparer<TInLeft, TInRight>(
                    new[] { new SortCriteria<TInLeft>(leftKey) }.ToList(),
                    new[] { new SortCriteria<TInRight>(rightKey) }.ToList()
                ),
                selector);
        }
    }
}
