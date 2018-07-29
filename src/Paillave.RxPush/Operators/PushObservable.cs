using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.RxPush.Operators
{
    public class PushObservable
    {
        public static IDeferedPushObservable<int> Range(int from, int count, bool startOnFirstSubscription = false)
        {
            return new DeferedPushObservable<int>((pushValue) =>
            {
                for (int i = 0; i < count; i++)
                    pushValue(from + i);
            }, startOnFirstSubscription);
        }
        public static IPushObservable<T> Merge<T>(params IPushObservable<T>[] pushObservables)
        {
            return new MergeSubject<T>(pushObservables);
        }
        public static IPushObservable<TOut> CombineWithLatest<TIn1, TIn2, TOut>(IPushObservable<TIn1> pushObservable1, IPushObservable<TIn2> pushObservable2, Func<TIn1, TIn2, TOut> selector)
        {
            return new CombineWithLatestSubject<TIn1, TIn2, TOut>(pushObservable1, pushObservable2, selector);
        }
        public static IPushObservable<TOut> Empty<TOut>()
        {
            return new DeferedPushObservable<TOut>(i => { }, true);
        }
    }
}
