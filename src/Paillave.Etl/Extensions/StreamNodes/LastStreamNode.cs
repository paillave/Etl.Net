using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class LastArgs<TOut>
    {
        public IStream<TOut> Input { get; set; }
    }
    public class LastStreamNode<TOut>(string name, LastArgs<TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, LastArgs<TOut>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISingleStream<TOut> CreateOutputStream(LastArgs<TOut> args) => 
            base.CreateSingleStream(args.Input.Observable.Last());
    }
}
