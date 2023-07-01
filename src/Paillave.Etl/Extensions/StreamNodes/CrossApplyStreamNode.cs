using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core
{
    public class CrossApplyArgs<TIn, TOut>
    {
        public bool NoParallelisation { get; set; } = true;
        public IStream<TIn> Stream { get; set; }
        public IValuesProvider<TIn, TOut> ValuesProvider { get; set; }
    }
    public class CrossApplyStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, CrossApplyArgs<TIn, TOut>>
    {
        public CrossApplyStreamNode(string name, CrossApplyArgs<TIn, TOut> args) : base(name, args) { }
        public override string TypeName => $"Cross apply {this.Args.ValuesProvider.TypeName}";
        public override ProcessImpact PerformanceImpact => this.Args.ValuesProvider.PerformanceImpact;
        public override ProcessImpact MemoryFootPrint => this.Args.ValuesProvider.MemoryFootPrint;
        protected override IStream<TOut> CreateOutputStream(CrossApplyArgs<TIn, TOut> args)
        {
            if (args.NoParallelisation)
            {
                return base.CreateUnsortedStream(args.Stream.Observable.MultiMap<TIn, TOut>(
                    (TIn i, Action<TOut> push) => args.ValuesProvider.PushValues(i, push, args.Stream.Observable.CancellationToken, base.ExecutionContext)));
            }
            else
            {
                var synchronizer = new Synchronizer();
                return base.CreateUnsortedStream(args.Stream.Observable.FlatMap((i, ct) =>
                {
                    return new DeferredPushObservable<TOut>((push, c) =>
                    {
                        using (synchronizer.WaitBeforeProcess())
                            args.ValuesProvider.PushValues(i, push, c, base.ExecutionContext);
                    }, ct);
                }));
            }
        }
    }
}
