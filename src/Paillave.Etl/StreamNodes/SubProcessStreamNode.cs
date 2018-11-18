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
        public Func<ISingleStream<TIn>, IStream<TOut>> SubProcess { get; set; }
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
            Synchronizer synchronizer = new Synchronizer(args.NoParallelisation);
            var outputObservable = args.Stream.Observable
                .FlatMap(i =>
                {
                    EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    var inputStream = new SingleStream<TIn>(this.Tracer, this.ExecutionContext, this.NodeName, PushObservable.FromSingle(i, waitHandle));
                    var outputStream = args.SubProcess(inputStream);
                    this.ExecutionContext.AddToWaitForCompletion(this.NodeName, outputStream.Observable);
                    IDisposable awaiter = null;
                    outputStream.Observable.Subscribe(j => { }, () => awaiter?.Dispose());
                    return new DeferredWrapperPushObservable<TOut>(outputStream.Observable, () =>
                    {
                        awaiter = synchronizer.WaitBeforeProcess();
                        waitHandle.Set();
                    });
                });
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
