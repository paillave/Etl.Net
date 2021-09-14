using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            T latestValue = default(T);
            var tcs = new TaskCompletionSource<T>();
            var subscription = observable.Subscribe(v => latestValue = v, () => tcs.SetResult(latestValue));
            return tcs.Task.ContinueWith(i =>
            {
                subscription.Dispose();
                return i.Result;
            });


            // var mtxInit = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
            // var task = Task.Run(() =>
            // {
            //     T latestValue = default(T);
            //     var mtx = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
            //     using (observable.Subscribe(v => latestValue = v, () => mtx.Set()))
            //     {
            //         mtxInit.Set(); //only once the stream is listened, the task can be returned. Otherwise, some events can be missed
            //         mtx.WaitOne();
            //         return latestValue;
            //     }
            // });
            // //TODO: THIS IS THE SOURCE OF DELAY WHEN BUILDING ETL
            // mtxInit.WaitOne();
            // return task;
        }
        public static Task ToEndAsync<T>(this IPushObservable<T> observable)
        {
            var tcs = new TaskCompletionSource<T>();
            var subscription = observable.Subscribe(v => { }, () => tcs.SetResult(default));
            return tcs.Task.ContinueWith(i => subscription.Dispose());



            // Moni
            // var tmp = Guid.NewGuid();
            // // https://jonskeet.uk/csharp/threads/waithandles.html
            // var mtxInit = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);
            // Stopwatch stopwatch0 = Stopwatch.StartNew();
            // var task = Task.Run(() =>
            // {
            //     var mtx = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset);

            //     Stopwatch stopwatch = Stopwatch.StartNew();
            //     using (observable.Subscribe(null, () => mtx.Set()))
            //     {
            //         Console.WriteLine($"subs: {tmp}->{stopwatch.ElapsedMilliseconds}");
            //         mtxInit.Set(); //only once the stream is listened, the task can be returned. Otherwise, some events can be missed
            //         mtx.WaitOne();
            //     }
            // });
            // //TODO: THIS IS THE SOURCE OF DELAY WHEN BUILDING ETL
            // Console.WriteLine($"task0: {tmp}->{stopwatch0.ElapsedMilliseconds}");
            // mtxInit.WaitOne();
            // Console.WriteLine($"task1: {tmp}->{stopwatch0.ElapsedMilliseconds}");
            // return task;
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
        public static IPushObservable<TResult[]> PairWithPrevious<TResult>(this IPushObservable<TResult> sourceS, int count)
        {
            return sourceS.Scan<TResult, TResult[]>(null, (a, v) => new TResult[] { v, a == null ? default : a[0] });
        }
    }
}
