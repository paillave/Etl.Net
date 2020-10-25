using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;
using System.Linq.Expressions;
using System.Threading;

namespace Paillave.Etl.Reactive.Operators
{
    public class LeftJoinSubject<TInLeft, TInRight, TOut> : PushSubject<TOut>
    {
        private IDisposable _leftSubscription;
        private IDisposable _rightSubscription;
        private object _sync = new object();

        private class Side<T>
        {
            public bool IsComplete { get; set; } = false;
            private Queue<T> _queue = new Queue<T>();
            public bool IsEmpty { get { return _queue.Count == 0; } }
            public T CurrentValue { get { return _queue.Peek(); } }
            public T Dequeue() => _queue.Dequeue();
            public void Enqueue(T element) => _queue.Enqueue(element);
        }

        private void TryUnstackQueues(Side<TInLeft> lSide, Side<TInRight> rSide, LeftJoinParams<TInLeft, TInRight, TOut> leftJoinParams)
        {
            bool somethingChanged;
            do
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    break;
                }
                somethingChanged = false;
                while (!rSide.IsEmpty && !lSide.IsEmpty && leftJoinParams.comparer.Compare(lSide.CurrentValue, rSide.CurrentValue) > 0)
                {
                    if (CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    rSide.Dequeue();
                    somethingChanged = true;
                }

                int comparison;
                while (!lSide.IsEmpty && !rSide.IsEmpty && (comparison = leftJoinParams.comparer.Compare(lSide.CurrentValue, rSide.CurrentValue)) <= 0)
                {
                    if (CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    this.TryPushValue(() => leftJoinParams.selector(lSide.Dequeue(), comparison == 0 ? rSide.CurrentValue : default(TInRight)));
                    somethingChanged = true;
                }

                if (rSide.IsEmpty && rSide.IsComplete)
                    while (!lSide.IsEmpty)
                    {
                        if (CancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        this.TryPushValue(() => leftJoinParams.selector(lSide.Dequeue(), default(TInRight)));
                        somethingChanged = true;
                    }
            } while (somethingChanged);
            if (lSide.IsComplete && (lSide.IsEmpty || rSide.IsComplete)) this.Complete();
        }

        public LeftJoinSubject(IPushObservable<TInLeft> leftS, IPushObservable<TInRight> rightS, LeftJoinParams<TInLeft, TInRight, TOut> leftJoinParams) : base(CancellationTokenSource.CreateLinkedTokenSource(leftS.CancellationToken, rightS.CancellationToken).Token)
        {
            var leftSide = new Side<TInLeft>();
            var rightSide = new Side<TInRight>();

            _leftSubscription = leftS.Subscribe(
                    (leftValue) =>
                    {
                        if (CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        lock (_sync)
                        {
                            leftSide.Enqueue(leftValue);
                            TryUnstackQueues(leftSide, rightSide, leftJoinParams);
                        }
                    },
                    () =>
                    {
                        lock (_sync)
                        {
                            leftSide.IsComplete = true;
                            TryUnstackQueues(leftSide, rightSide, leftJoinParams);
                        }
                    }
                );
            _rightSubscription = rightS.Subscribe(
                    (rightValue) =>
                    {
                        if (CancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        lock (_sync)
                        {
                            rightSide.Enqueue(rightValue);
                            TryUnstackQueues(leftSide, rightSide, leftJoinParams);
                        }
                    },
                    () =>
                    {
                        lock (_sync)
                        {
                            rightSide.IsComplete = true;
                            TryUnstackQueues(leftSide, rightSide, leftJoinParams);
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
    public class LeftJoinParams<TInLeft, TInRight, TOut>
    {
        public IComparer<TInLeft, TInRight> comparer { get; set; }
        public Func<TInLeft, TInRight, TOut> selector { get; set; }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> LeftJoin<TInLeft, TInRight, TOut>(this IPushObservable<TInLeft> observable, IPushObservable<TInRight> rightS, IComparer<TInLeft, TInRight> comparer, Func<TInLeft, TInRight, TOut> selector)
        {
            return new LeftJoinSubject<TInLeft, TInRight, TOut>(observable, rightS, new LeftJoinParams<TInLeft, TInRight, TOut> { comparer = comparer, selector = selector });
        }
        public static IPushObservable<TOut> LeftJoin<TInLeft, TInRight, TOut, TKey>(this IPushObservable<TInLeft> observable, IPushObservable<TInRight> rightS, Func<TInLeft, TKey> leftKey, Func<TInRight, TKey> rightKey, object keyPositions, Func<TInLeft, TInRight, TOut> selector)
        {
            return new LeftJoinSubject<TInLeft, TInRight, TOut>(
                observable,
                rightS,
                new LeftJoinParams<TInLeft, TInRight, TOut>
                {
                    comparer = new SortDefinitionComparer<TInLeft, TInRight, TKey>(
                        new SortDefinition<TInLeft, TKey>(leftKey, keyPositions),
                        new SortDefinition<TInRight, TKey>(rightKey, keyPositions)
                    ),
                    selector = selector
                });
        }
    }
}
