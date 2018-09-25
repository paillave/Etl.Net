using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.StreamNodes
{
    public class UnionAllArgs<TIn>
    {
        public IStream<TIn> Stream1 { get; set; }
        public IStream<TIn> Stream2 { get; set; }
    }
    public class UnionAllStreamNode<TIn> : StreamNodeBase<TIn, IStream<TIn>, UnionAllArgs<TIn>>
    {
        public UnionAllStreamNode(string name, UnionAllArgs<TIn> args) : base(name, args)
        {
        }

        protected override IStream<TIn> CreateOutputStream(UnionAllArgs<TIn> args)
        {
            return base.CreateUnsortedStream(args.Stream1.Observable.Concatenate(args.Stream2.Observable));
        }
    }



    // public class ToSubProcessUnionAllArgs<TIn, TOut>
    // {
    //     public IStream<TIn> Stream { get; set; }
    //     public IEnumerable<Func<IStream<TIn>, IStream<TOut>>> SubProcesses { get; set; }
    //     public bool NoParallelisation { get; set; }
    // }
    // public class ToSubProcessUnionAllStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, ToSubProcessUnionAllArgs<TIn, TOut>>
    // {
    //     public ToSubProcessUnionAllStreamNode(string name, ToSubProcessUnionAllArgs<TIn, TOut> args) : base(name, args)
    //     {
    //     }

    //     protected override IStream<TOut> CreateOutputStream(ToSubProcessUnionAllArgs<TIn, TOut> args)
    //     {
    //         Semaphore semaphore = args.NoParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
    //         var outputObservable = args.Stream.Observable.First().FlatMap(i =>
    //             {
    //                 // IStream<TOut> outputStream = null;
    //                 EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
    //                 var inputStream = new Stream<TIn>(this.Tracer, this.ExecutionContext, this.NodeName, PushObservable.FromSingle(i, waitHandle));
    //                 var observables = new List<IPushObservable<TOut>>();
    //                 IPushObservable<TOut> observable = null;
    //                 foreach (var subProcess in args.SubProcesses)
    //                 {
    //                     var obs = subProcess(inputStream).Observable;
    //                     if (observable == null) observable.Merge(obs);
    //                     observables.Add(obs);
    //                 }
    //                 return observable;
    //                 foreach (var subProcess in args.SubProcesses)
    //                 {
    //                     if (outputStream == null)
    //                         outputStream = subProcess(inputStream);
    //                     else
    //                         outputStream = outputStream.Observable.Concatenate<TOut>(inputStream.Observable);
    //                     this.ExecutionContext.AddToWaitForCompletion(this.NodeName, outputStream.Observable);
    //                     outputStream.Observable.Subscribe(j => { }, () => semaphore.Release());
    //                     var endOfChainObservable = new DeferedWrapperPushObservable<TOut>(outputStream.Observable, () =>
    //                       {
    //                           semaphore.WaitOne();
    //                           waitHandle.Set();
    //                       });
    //                 }
    //             });
    //         return base.CreateUnsortedStream(outputObservable);
    //     }
    // }
}
