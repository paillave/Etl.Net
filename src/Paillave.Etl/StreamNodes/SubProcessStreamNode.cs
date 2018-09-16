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
    public class SubProcessArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<IStream<TIn>, IStream<TOut>> SimpleSubProcess { get; set; }
        public bool NoParallelisation { get; set; }
    }
    public class SubProcessStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, SubProcessArgs<TIn, TOut>>
    {
        public override bool IsAwaitable => true;
        public SubProcessStreamNode(string name, SubProcessArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(SubProcessArgs<TIn, TOut> args)
        {
            Semaphore semaphore = args.NoParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
            var outputObservable = args.Stream.Observable.FlatMap(i =>
                {
                    EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    var inputStream = new Stream<TIn>(this.Tracer, this.ExecutionContext, this.NodeName, PushObservable.FromSingle(i, waitHandle));
                    var outputStream = args.SimpleSubProcess(inputStream);
                    var intermediateOutputObservable = outputStream.Observable;
                    this.ExecutionContext.AddToWaitForCompletion(this.NodeName, intermediateOutputObservable);
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
