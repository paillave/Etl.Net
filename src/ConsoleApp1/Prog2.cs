using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reactive.Disposables;

namespace ConsoleApplication24
{
    class Program2
    {
        static void MainOld(string[] args)
        {
            var elementS = new[] {
                //new { Id = 1, ExtId = 1 },
                //new { Id = 2, ExtId = 1 },
                //new { Id = 3, ExtId = 1 },
                //new { Id = 1, ExtId = 1 },
                //new { Id = 6, ExtId = 1 },
                new { Id = 4, ExtId = 2 },
                new { Id = 5, ExtId = 2 },
                new { Id = 6, ExtId = 2 },
                new { Id = 3, ExtId = 2 },
                new { Id = 2, ExtId = 2 },
                new { Id = 4, ExtId = 3 },
                new { Id = 5, ExtId = 3 },
                new { Id = 7, ExtId = 4 },
            }
                //.OrderBy(i => i.ExtId)
                .ToObservable();
            var elementTypeS = Enumerable
                .Range(1, 4)
                .Where(i => i % 2 == 0)
                .Select(i => new { Id = i, Name = $"Name {i}" })
                .ToObservable();

            elementS
                .LeftJoin(elementTypeS, l => l.ExtId, r => r.Id, (l, r) => new { l.Id, l.ExtId, ExtName = r?.Name ?? "<null>" })
                //.PairWithPrevious()
                //.ForkJoin()
                //.Where(i => i.Item1.ExtId != i.Item2.ExtId)
                .Select(i => $"\t\t{i.Id}-{i.ExtId}-{i.ExtName}")
                .Subscribe(Console.WriteLine);

            Console.WriteLine("\t\tDone");
            Console.ReadKey();
        }
    }


    // https://stackoverflow.com/questions/4911465/how-to-join-multiple-iobservable-sequences
    public static class ObservableExtension
    {
        public static IObservable<Tuple<TResult, TResult>> PairWithPrevious<TResult>(this IObservable<TResult> sourceS)
        {
            return sourceS.Scan<TResult, Tuple<TResult, TResult>>(null, (a, v) => new Tuple<TResult, TResult>(a == null ? v : a.Item2, v));
        }
        private class Side<T, K> where K : IComparable<K>
        {
            public Side(Func<T, K> getKey)
            {
                this.GetKey = getKey;
            }
            public bool IsComplete { get; set; } = false;
            private Queue<T> _queue = new Queue<T>();
            public Func<T, K> GetKey { get; }
            public bool IsEmpty { get { return _queue.Count == 0; } }
            public T CurrentValue { get { return _queue.Peek(); } }
            public K CurrentKey { get { return GetKey(_queue.Peek()); } }
            public T Dequeue() => _queue.Dequeue();
            public void Enqueue(T element) => _queue.Enqueue(element);
            public int CompareTo(K side2key) => GetKey(CurrentValue).CompareTo(side2key);
        }
        public static IObservable<TOut> LeftJoin<TInLeft, TInRight, TKey, TOut>(this IObservable<TInLeft> leftS, IObservable<TInRight> rightS, Func<TInLeft, TKey> leftKey, Func<TInRight, TKey> rightKey, Func<TInLeft, TInRight, TOut> selector) where TKey : IComparable<TKey>
        {
            return Observable.Create<TOut>(observer =>
            {
                var leftSide = new Side<TInLeft, TKey>(leftKey);
                var rightSide = new Side<TInRight, TKey>(rightKey);

                object gate = new object();

                Action TryUnstackQueues = () =>
                {
                    bool somethingChanged;
                    do
                    {
                        somethingChanged = false;
                        while (!rightSide.IsEmpty && !leftSide.IsEmpty && leftSide.CompareTo(rightSide.CurrentKey) > 0)
                        {
                            rightSide.Dequeue();
                            somethingChanged = true;
                        }

                        int comparison;
                        while (!leftSide.IsEmpty && !rightSide.IsEmpty && (comparison = leftSide.CompareTo(rightSide.CurrentKey)) <= 0)
                        {
                            observer.OnNext(selector(leftSide.Dequeue(), comparison == 0 ? rightSide.CurrentValue : default(TInRight)));
                            somethingChanged = true;
                        }

                        if (rightSide.IsEmpty && rightSide.IsComplete)
                            while (!leftSide.IsEmpty)
                            {
                                observer.OnNext(selector(leftSide.Dequeue(), default(TInRight)));
                                somethingChanged = true;
                            }
                    } while (somethingChanged);
                    if (leftSide.IsComplete && rightSide.IsComplete) observer.OnCompleted();
                };
                SingleAssignmentDisposable leftSubscription = new SingleAssignmentDisposable
                {
                    Disposable = leftS.Subscribe(
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
                    )
                };
                SingleAssignmentDisposable rightSubscription = new SingleAssignmentDisposable
                {
                    Disposable = rightS.Subscribe(
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
                    )
                };

                return new CompositeDisposable(leftSubscription, rightSubscription);
            });
        }
    }
}
