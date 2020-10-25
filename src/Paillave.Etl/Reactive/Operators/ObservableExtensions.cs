using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public static partial class ObservableExtensions
    {
        public static Task<T> LastAsync<T>(this IPushObservable<T> observable)
        {
            return ToTaskAsync(observable);
        }
        public static Task<T> ToTaskAsync<T>(this IPushObservable<T> observable)
        {
            var guid = Guid.NewGuid();
            var mtxInit = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
            var task = Task.Run(() =>
            {
                T latestValue = default(T);
                var mtx = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
                using (observable.Subscribe(
                    v => latestValue = v,
                    () => mtx.Set()))
                {
                    mtxInit.Set(); //only once the stream is listened, the task can be returned. Otherwise, some events can be missed
                    var tmp = guid;
                    mtx.WaitOne();
                    return latestValue;
                }
            });
            mtxInit.WaitOne();
            return task;
        }
        public static IPushObservable<T> DelayTillEndOfStream<T>(this IPushObservable<T> observable)
        {
            return observable.ToList().FlatMap((i, ct) => PushObservable.FromEnumerable(i, ct));
        }
        public static Task<List<T>> ToListAsync<T>(this IPushObservable<T> observable)
        {
            return observable.ToList().ToTaskAsync();
        }
        public static IPushObservable<int> Count<T>(this IPushObservable<T> observable)
        {
            return observable.Aggregate<T, int>((acc, red) => acc + 1);
        }
        public static IPushObservable<int> Count<T>(this IPushObservable<T> observable, Func<T, bool> criteria)
        {
            return observable.Filter(criteria).Aggregate<T, int>((acc, red) => acc + 1);
        }
        public static IPushObservable<Tuple<TResult, TResult>> PairWithPrevious<TResult>(this IPushObservable<TResult> sourceS)
        {
            return sourceS.Scan<TResult, Tuple<TResult, TResult>>(null, (a, v) => new Tuple<TResult, TResult>(a == null ? v : a.Item2, v));
        }
    }
}
