using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class UnsetForCorrelationArgs<TIn>
    {
        public IStream<Correlated<TIn>> Input { get; set; }
    }
    public class UnsetForCorrelationStreamNode<TIn> : StreamNodeBase<TIn, IStream<TIn>, UnsetForCorrelationArgs<TIn>>
    {
        public UnsetForCorrelationStreamNode(string name, UnsetForCorrelationArgs<TIn> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TIn> CreateOutputStream(UnsetForCorrelationArgs<TIn> args)
        {
            return base.CreateUnsortedStream(args.Input.Observable.Map(i => i.Row));
        }
    }
}
