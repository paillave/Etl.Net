using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
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

        protected override TOutStream CreateOutputStream(WhereArgs<TOut, TOutStream> args)
        {
            return base.CreateMatchingStream(args.Input.Observable.Filter(args.Predicate), args.Input);
        }
    }
}
