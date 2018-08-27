using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
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

        protected override TOutStream CreateOutputStream(TopArgs<TOut, TOutStream> args)
        {
            return base.CreateMatchingStream(args.Input.Observable.Take(args.Count), args.Input);
        }
    }
}
