using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.StreamNodes
{
    public class OfTypeArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
    }
    public class OfTypeStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, OfTypeArgs<TIn, TOut>> where TOut : TIn
    {
        public OfTypeStreamNode(string name, OfTypeArgs<TIn, TOut> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override IStream<TOut> CreateOutputStream(OfTypeArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.OfType<TIn, TOut>();
            return base.CreateUnsortedStream(obs);
        }
    }
}
