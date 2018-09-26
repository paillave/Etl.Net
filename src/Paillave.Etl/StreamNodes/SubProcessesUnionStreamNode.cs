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
    public class SubProcessesUnionArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public IEnumerable<Func<IStream<TIn>, IStream<TOut>>> SubProcesses { get; set; }
        public bool NoParallelisation { get; set; }
    }
    public class SubProcessesUnionStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, SubProcessesUnionArgs<TIn, TOut>>
    {
        public SubProcessesUnionStreamNode(string name, SubProcessesUnionArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(SubProcessesUnionArgs<TIn, TOut> args)
        {
            Semaphore semaphore = args.NoParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
            var outputObservable = args.Stream.Observable
                .First()
                .FlatMap(i => PushObservable.FromEnumerable(args.SubProcesses.Select(sp => new
                {
                    cnfg = i,
                    subProc = sp
                })))
                .FlatMap(i =>
                {
                    EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    var inputStream = new Stream<TIn>(this.Tracer, this.ExecutionContext, this.NodeName, PushObservable.FromSingle(i.cnfg, waitHandle));
                    var outputStream = i.subProc(inputStream);
                    outputStream.Observable.Subscribe(j => { }, () => semaphore.Release());
                    return new DeferedWrapperPushObservable<TOut>(outputStream.Observable, () =>
                    {
                        semaphore.WaitOne();
                        waitHandle.Set();
                    });
                });
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
