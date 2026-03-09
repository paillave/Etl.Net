using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace Paillave.Etl.Core;

public class ToSubProcessesArgs<TIn, TOut>
{
    public IStream<TIn> Stream { get; set; }
    public IEnumerable<Func<ISingleStream<TIn>, IStream<TOut>>> SubProcesses { get; set; }
    public bool NoParallelisation { get; set; }
}

public class ToSubProcessesStreamNode<TIn, TOut>(string name, ToSubProcessesArgs<TIn, TOut> args) : StreamNodeBase<TOut, IStream<TOut>, ToSubProcessesArgs<TIn, TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<TOut> CreateOutputStream(ToSubProcessesArgs<TIn, TOut> args)
    {
        // Semaphore semaphore = args.NoParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
        Synchronizer synchronizer = new(args.NoParallelisation);

        if (this.ExecutionContext is GetDefinitionExecutionContext)
        {
            var inputStream = new SingleStream<TIn>(new ChildNodeWrapper( this), PushObservable.FromSingle(default(TIn), args.Stream.Observable.CancellationToken));
            foreach (var subProcess in args.SubProcesses)
            {
                var outputStream = subProcess(inputStream);
                this.ExecutionContext.AddNode(this, outputStream.Observable);
            }
        }

        var outputObservable = args.Stream.Observable
            .FlatMap((i, ct) => PushObservable.FromEnumerable(args.SubProcesses.Select(sp => new
            {
                config = i,
                subProc = sp
            }), ct))
            .FlatMap((i, ct) =>
            {
                EventWaitHandle waitHandle = new(false, EventResetMode.ManualReset);
                var inputStream = new SingleStream<TIn>(new ChildNodeWrapper( this), PushObservable.FromSingle(i.config, waitHandle, ct));
                var outputStream = i.subProc(inputStream);
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
