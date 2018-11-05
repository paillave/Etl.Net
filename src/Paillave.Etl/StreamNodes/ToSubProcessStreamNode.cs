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
    public class ToSubProcessArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<ISingleStream<TIn>, IStream<TOut>> SubProcess { get; set; }
        public bool NoParallelisation { get; set; }
    }
    //TODO:what it the difference between SubProcessesUnionStreamNode and ToSubProcessStreamNode?
    //TODO: rename to Through...?
    public class ToSubProcessStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, ToSubProcessArgs<TIn, TOut>>
    {
        public override bool IsAwaitable => true;
        public ToSubProcessStreamNode(string name, ToSubProcessArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(ToSubProcessArgs<TIn, TOut> args)
        {
            //TODO:replace with suitable api if necessary
            Semaphore semaphore = args.NoParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
            var outputObservable = args.Stream.Observable.FlatMap(i =>
                {
                    EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    var inputStream = new SingleStream<TIn>(this.Tracer, this.ExecutionContext, this.NodeName, PushObservable.FromSingle(i, waitHandle));
                    var outputStream = args.SubProcess(inputStream);
                    this.ExecutionContext.AddToWaitForCompletion(this.NodeName, outputStream.Observable);
                    outputStream.Observable.Subscribe(j => { }, () => semaphore.Release());
                    return new DeferredWrapperPushObservable<TOut>(outputStream.Observable, () =>
                    {
                        semaphore.WaitOne();
                        waitHandle.Set();
                    });
                });
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
