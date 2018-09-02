using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class UnionArgs<TIn>
    {
        public IStream<TIn> Stream1 { get; set; }
        public IStream<TIn> Stream2 { get; set; }
    }
    public class UnionStreamNode<TIn> : StreamNodeBase<TIn, IStream<TIn>, UnionArgs<TIn>>
    {
        public UnionStreamNode(string name, UnionArgs<TIn> args) : base(name, args)
        {
        }

        protected override IStream<TIn> CreateOutputStream(UnionArgs<TIn> args)
        {
            return base.CreateUnsortedStream(args.Stream1.Observable.Merge(args.Stream2.Observable));
        }
    }
}
