using System;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Core.System;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;

namespace System.Reactive.Linq
{
    public static class ObservableEx
    {
        //public static IPushObservable<Tuple<TResult, TResult>> PairWithPrevious<TResult>(this IPushObservable<TResult> sourceS)
        //{
        //    return sourceS.Scan<TResult, Tuple<TResult, TResult>>((a, v) => new Tuple<TResult, TResult>(a == null ? v : a.Item2, v), null);
        //}

        //private static Func<TIn, ErrorManagementItem<TIn, TOut>> ErrorManagementWrapFunction<TIn, TOut>(Func<TIn, TOut> call)
        //{
        //    return (TIn input) =>
        //    {
        //        try
        //        {
        //            return new ErrorManagementItem<TIn, TOut>(input, call(input));
        //        }
        //        catch (Exception ex)
        //        {
        //            return new ErrorManagementItem<TIn, TOut>(input, ex);
        //        }
        //    };
        //}
        //public static IPushObservable<TOut> SelectCatch<TIn, TOut>(this IPushObservable<TIn> source, Func<TIn, TOut> selector)
        //{
        //    return Observable.Create<TOut>(o =>
        //    {
        //        var disp = source.Select(ErrorManagementWrapFunction(selector)).Subscribe(i =>
        //        {
        //            if (i.OnException)
        //                o.OnError(i.Exception);
        //            else
        //                o.OnNext(i.Output);
        //        }, o.OnError);
        //        return new Disposables.CompositeDisposable(disp);
        //    });
        //}

        //private class Side<T>
        //{
        //    public bool IsComplete { get; set; } = false;
        //    private Queue<T> _queue = new Queue<T>();
        //    public bool IsEmpty { get { return _queue.Count == 0; } }
        //    public T CurrentValue { get { return _queue.Peek(); } }
        //    public T Dequeue() => _queue.Dequeue();
        //    public void Enqueue(T element) => _queue.Enqueue(element);
        //}
        //public static IPushObservable<TOut> LeftJoin<TInLeft, TInRight, TKey, TOut>(this IPushObservable<TInLeft> leftS, IPushObservable<TInRight> rightS, Func<TInLeft, TKey> leftKey, Func<TInRight, TKey> rightKey, Func<TInLeft, TInRight, TOut> selector) where TKey : IComparable<TKey>
        //public static IPushObservable<TOut> LeftJoin<TInLeft, TInRight, TOut>(this IPushObservable<TInLeft> leftS, IPushObservable<TInRight> rightS, IComparer<TInLeft, TInRight> comparer, Func<TInLeft, TInRight, TOut> selector)
        //{
        //    return Observable.Create<TOut>(observer =>
        //    {
        //        var leftSide = new Side<TInLeft>();
        //        var rightSide = new Side<TInRight>();

        //        object gate = new object();

        //        Action TryUnstackQueues = () =>
        //        {
        //            bool somethingChanged;
        //            do
        //            {
        //                somethingChanged = false;
        //                while (!rightSide.IsEmpty && !leftSide.IsEmpty && comparer.Compare(leftSide.CurrentValue, rightSide.CurrentValue) > 0)
        //                {
        //                    rightSide.Dequeue();
        //                    somethingChanged = true;
        //                }

        //                int comparison;
        //                while (!leftSide.IsEmpty && !rightSide.IsEmpty && (comparison = comparer.Compare(leftSide.CurrentValue, rightSide.CurrentValue)) <= 0)
        //                {
        //                    observer.OnNext(selector(leftSide.Dequeue(), comparison == 0 ? rightSide.CurrentValue : default(TInRight)));
        //                    somethingChanged = true;
        //                }

        //                if (rightSide.IsEmpty && rightSide.IsComplete)
        //                    while (!leftSide.IsEmpty)
        //                    {
        //                        observer.OnNext(selector(leftSide.Dequeue(), default(TInRight)));
        //                        somethingChanged = true;
        //                    }
        //            } while (somethingChanged);
        //            if (leftSide.IsComplete && rightSide.IsComplete) observer.OnCompleted();
        //        };
        //        SingleAssignmentDisposable leftSubscription = new SingleAssignmentDisposable
        //        {
        //            Disposable = leftS.Subscribe(
        //                (leftValue) =>
        //                {
        //                    lock (gate)
        //                    {
        //                        leftSide.Enqueue(leftValue);
        //                        TryUnstackQueues();
        //                    }
        //                },
        //                () =>
        //                {
        //                    lock (gate)
        //                    {
        //                        leftSide.IsComplete = true;
        //                        TryUnstackQueues();
        //                    }
        //                }
        //            )
        //        };
        //        SingleAssignmentDisposable rightSubscription = new SingleAssignmentDisposable
        //        {
        //            Disposable = rightS.Subscribe(
        //                (rightValue) =>
        //                {
        //                    lock (gate)
        //                    {
        //                        rightSide.Enqueue(rightValue);
        //                        TryUnstackQueues();
        //                    }
        //                },
        //                () =>
        //                {
        //                    lock (gate)
        //                    {
        //                        rightSide.IsComplete = true;
        //                        TryUnstackQueues();
        //                    }
        //                }
        //            )
        //        };

        //        return new CompositeDisposable(leftSubscription, rightSubscription);
        //    });
        //}
    }
}
