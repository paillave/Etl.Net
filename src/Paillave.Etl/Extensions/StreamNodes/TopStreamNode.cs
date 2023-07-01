using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class TopArgs<TOut, TOutStream> where TOutStream : IStream<TOut>
    {
        public TOutStream Input { get; set; }
        public int Count { get; set; }
    }
    public class TopStreamNode<TOut, TOutStream> : StreamNodeBase<TOut, TOutStream, TopArgs<TOut, TOutStream>> where TOutStream : IStream<TOut>
    {
        public TopStreamNode(string name, TopArgs<TOut, TOutStream> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override TOutStream CreateOutputStream(TopArgs<TOut, TOutStream> args)
        {
            return base.CreateMatchingStream(args.Input.Observable.Take(args.Count), args.Input);
        }
    }
}
