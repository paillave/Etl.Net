using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;

namespace Paillave.RxPush.Operators
{
    public static partial class ObservableExtensions
    {
        public static Task<T> LastAsync<T>(this IPushObservable<T> observable)
        {
            return ToTaskAsync(observable);
        }
        public static Task<T> ToTaskAsync<T>(this IPushObservable<T> observable)
        {
            return Task.Run(() =>
            {
                T latestValue = default(T);
                var mtx = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
                var disp = observable.Subscribe((v) => latestValue = v, () => mtx.Set());
                mtx.WaitOne();
                disp.Dispose();
                return latestValue;
            });
        }
        public static IPushObservable<T> DelayTillEndOfStream<T>(this IPushObservable<T> observable)
        {
            return observable.ToList().FlatMap(i => PushObservable.FromEnumerable(i));
        }
        public static Task<List<T>> ToListAsync<T>(this IPushObservable<T> observable)
        {
            return observable.ToList().ToTaskAsync();
        }
        public static IPushObservable<int> Count<T>(this IPushObservable<T> observable)
        {
            return observable.Scan((acc, red) => acc + 1, 0).Last();
        }
        public static IPushObservable<int> Count<T>(this IPushObservable<T> observable, Func<T, bool> criteria)
        {
            return observable.Filter(criteria).Scan<T, int>((acc, red) => acc + 1, 0).Last();
        }
        public static IPushObservable<Tuple<TResult, TResult>> PairWithPrevious<TResult>(this IPushObservable<TResult> sourceS)
        {
            return sourceS.Scan<TResult, Tuple<TResult, TResult>>((a, v) => new Tuple<TResult, TResult>(a == null ? v : a.Item2, v), null);
        }
    }
}
