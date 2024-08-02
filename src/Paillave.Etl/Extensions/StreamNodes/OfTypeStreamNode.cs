using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class OfTypeArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
    }
    public class OfTypeStreamNode<TIn, TOut>(string name, OfTypeArgs<TIn, TOut> args) : StreamNodeBase<TOut, IStream<TOut>, OfTypeArgs<TIn, TOut>>(name, args) where TOut : TIn
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override IStream<TOut> CreateOutputStream(OfTypeArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.OfType<TIn, TOut>();
            return base.CreateUnsortedStream(obs);
        }
    }
}
