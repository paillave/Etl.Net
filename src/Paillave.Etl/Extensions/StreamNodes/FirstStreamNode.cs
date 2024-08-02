using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class FirstArgs<TOut>
    {
        public IStream<TOut> Input { get; set; }
    }
    public class FirstStreamNode<TOut>(string name, FirstArgs<TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, FirstArgs<TOut>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISingleStream<TOut> CreateOutputStream(FirstArgs<TOut> args) =>
            base.CreateSingleStream(args.Input.Observable.First());
    }
}
