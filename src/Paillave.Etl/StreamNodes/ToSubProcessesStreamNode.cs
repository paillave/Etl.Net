using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.StreamNodes
{
    public class ToSubProcessesArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public IEnumerable<Func<ISingleStream<TIn>, IStream<TOut>>> SubProcesses { get; set; }
        public bool NoParallelisation { get; set; }
    }

    public class ToSubProcessesStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, ToSubProcessesArgs<TIn, TOut>>
    {
        public ToSubProcessesStreamNode(string name, ToSubProcessesArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(ToSubProcessesArgs<TIn, TOut> args)
        {
            // Semaphore semaphore = args.NoParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
            Synchronizer synchronizer = new Synchronizer(args.NoParallelisation);

            var outputObservable = args.Stream.Observable
                .FlatMap(i => PushObservable.FromEnumerable(args.SubProcesses.Select(sp => new
                {
                    config = i,
                    subProc = sp
                })))
                .FlatMap(i =>
                {
                    EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    var inputStream = new SingleStream<TIn>(this.Tracer, this.ExecutionContext, this.NodeName, PushObservable.FromSingle(i.config, waitHandle));
                    var outputStream = i.subProc(inputStream);
                    IDisposable awaiter = null;
                    outputStream.Observable.Subscribe(j => { }, () => awaiter?.Dispose());
                    return new DeferredWrapperPushObservable<TOut>(outputStream.Observable, () =>
                    {
                        awaiter = synchronizer.WaitBeforeProcess();
                        waitHandle.Set();
                    });
                    // outputStream.Observable.Subscribe(j => { }, () => semaphore.Release());
                    // return new DeferredWrapperPushObservable<TOut>(outputStream.Observable, () =>
                    // {
                    //     semaphore.WaitOne();
                    //     waitHandle.Set();
                    // });
                });
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
