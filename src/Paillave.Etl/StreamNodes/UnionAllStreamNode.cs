using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.StreamNodes
{
    public class UnionAllArgs<TIn>
    {
        public IStream<TIn> Stream1 { get; set; }
        public IStream<TIn> Stream2 { get; set; }
    }
    public class UnionAllStreamNode<TIn> : StreamNodeBase<TIn, IStream<TIn>, UnionAllArgs<TIn>>
    {
        public UnionAllStreamNode(string name, UnionAllArgs<TIn> args) : base(name, args)
        {
        }

        protected override IStream<TIn> CreateOutputStream(UnionAllArgs<TIn> args)
        {
            return base.CreateUnsortedStream(args.Stream1.Observable.Concatenate(args.Stream2.Observable));
        }
    }

}
