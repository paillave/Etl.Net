using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Operators
{
    public class PushObservable
    {
        public static IDeferedPushObservable<TOut> FromEnumerable<TOut>(IEnumerable<TOut> enumerable, WaitHandle startSynchronizer = null)
        {
            return new DeferedPushObservable<TOut>(pushValue =>
            {
                foreach (var item in enumerable)
                    pushValue(item);
            }, startSynchronizer);
        }
        public static IDeferedPushObservable<int> Range(int from, int count, WaitHandle startSynchronizer = null)
        {
            return FromEnumerable(Enumerable.Range(from, count), startSynchronizer);
        }
        public static IPushObservable<T> Merge<T>(params IPushObservable<T>[] pushObservables)
        {
            return new MergeSubject<T>(pushObservables);
        }
        public static IPushObservable<TOut> CombineWithLatest<TIn1, TIn2, TOut>(IPushObservable<TIn1> pushObservable1, IPushObservable<TIn2> pushObservable2, Func<TIn1, TIn2, TOut> selector)
        {
            return new CombineWithLatestSubject<TIn1, TIn2, TOut>(pushObservable1, pushObservable2, selector);
        }
        public static IDeferedPushObservable<TOut> Empty<TOut>(WaitHandle startSynchronizer = null)
        {
            return new DeferedPushObservable<TOut>(i => { }, startSynchronizer);
        }
    }
}
