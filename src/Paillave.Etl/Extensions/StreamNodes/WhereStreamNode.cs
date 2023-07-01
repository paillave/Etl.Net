using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core
{
    public class WhereArgs<TOut, TOutStream> where TOutStream : IStream<TOut>
    {
        public TOutStream Input { get; set; }
        public Func<TOut, bool> Predicate { get; set; }
    }
    public class WhereStreamNode<TOut, TOutStream> : StreamNodeBase<TOut, TOutStream, WhereArgs<TOut, TOutStream>> where TOutStream : IStream<TOut>
    {
        public WhereStreamNode(string name, WhereArgs<TOut, TOutStream> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override TOutStream CreateOutputStream(WhereArgs<TOut, TOutStream> args)
        {
            return base.CreateMatchingStream(args.Input.Observable.Filter(args.Predicate), args.Input);
        }
    }
}
