using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class TopArgs<TOut, TOutStream> where TOutStream : IStream<TOut>
    {
        public TOutStream Input { get; set; }
        public int Count { get; set; }
    }
    public class TopStreamNode<TOut, TOutStream>(string name, TopArgs<TOut, TOutStream> args) : StreamNodeBase<TOut, TOutStream, TopArgs<TOut, TOutStream>>(name, args) where TOutStream : IStream<TOut>
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override TOutStream CreateOutputStream(TopArgs<TOut, TOutStream> args) => 
            base.CreateMatchingStream(args.Input.Observable.Take(args.Count), args.Input);
    }
}
