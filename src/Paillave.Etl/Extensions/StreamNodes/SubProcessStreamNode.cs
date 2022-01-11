using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Threading;

namespace Paillave.Etl.Core
{
    public class SubProcessArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<ISingleStream<TIn>, IStream<TOut>> SubProcess { get; set; }
        public bool NoParallelisation { get; set; }
    }
    public class SubProcessStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, SubProcessArgs<TIn, TOut>>
    {
        public SubProcessStreamNode(string name, SubProcessArgs<TIn, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(SubProcessArgs<TIn, TOut> args)
        {
            if (this.ExecutionContext is GetDefinitionExecutionContext)
            {
                var inputStream = new SingleStream<TIn>(new SubNodeWrapper(this), PushObservable.FromSingle(default(TIn), args.Stream.Observable.CancellationToken));
                var outputStream = args.SubProcess(inputStream);
                // this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
            Synchronizer synchronizer = new Synchronizer(args.NoParallelisation);
            var outputObservable = args.Stream.Observable
                .FlatMap((i, ct) =>
                {
                    // TODO: Solve bug here (this operator makes the process freezing))

                    EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    var inputStream = new SingleStream<TIn>(new SubNodeWrapper(this), PushObservable.FromSingle(i, waitHandle, ct));
                    var outputStream = args.SubProcess(inputStream);
                    // this.ExecutionContext.AddNode(this, outputStream.Observable);
                    IDisposable awaiter = null;
                    outputStream.Observable.Subscribe(j => { }, () => awaiter?.Dispose());
                    return new DeferredWrapperPushObservable<TOut>(outputStream.Observable, () =>
                    {
                        awaiter = synchronizer.WaitBeforeProcess();
                        waitHandle.Set();
                    }, null, ct);
                });
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
