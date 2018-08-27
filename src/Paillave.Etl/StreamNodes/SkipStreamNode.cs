using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
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

        protected override TOutStream CreateOutputStream(SkipArgs<TOut, TOutStream> args)
        {
            return base.CreateMatchingStream(args.Input.Observable.Skip(args.Count), args.Input);
        }
    }
}
