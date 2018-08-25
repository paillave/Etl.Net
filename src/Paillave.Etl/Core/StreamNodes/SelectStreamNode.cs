using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class SelectArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<TIn, TOut> Selector { get; set; }
        public Func<TIn, int, TOut> IndexSelector { get; set; }
    }

    public class SelectStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, SelectArgs<TIn, TOut>>
    {
        public SelectStreamNode(string name, SelectArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(SelectArgs<TIn, TOut> args)
        {
            if (args.IndexSelector == null)
                return base.CreateStream(args.Stream.Observable.Map(WrapSelectForDisposal(args.Selector)));
            else
                return base.CreateStream(args.Stream.Observable.Map(WrapSelectIndexForDisposal(args.IndexSelector)));
        }
    }
}
