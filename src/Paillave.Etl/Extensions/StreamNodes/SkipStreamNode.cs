using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class SkipArgs<TOut, TOutStream> where TOutStream : IStream<TOut>
    {
        public TOutStream Input { get; set; }
        public int Count { get; set; }
    }
    public class SkipStreamNode<TOut, TOutStream> : StreamNodeBase<TOut, TOutStream, SkipArgs<TOut, TOutStream>> where TOutStream : IStream<TOut>
    {
        public SkipStreamNode(string name, SkipArgs<TOut, TOutStream> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override TOutStream CreateOutputStream(SkipArgs<TOut, TOutStream> args)
        {
            return base.CreateMatchingStream(args.Input.Observable.Skip(args.Count), args.Input);
        }
    }
}
