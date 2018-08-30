using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class SelectArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<TIn, TOut> Selector { get; set; }
        public Func<TIn, int, TOut> IndexSelector { get; set; }
        public bool ExcludeNull { get; set; }
    }

    public class SelectStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, SelectArgs<TIn, TOut>>
    {
        public SelectStreamNode(string name, SelectArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(SelectArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs;
            if (args.IndexSelector == null)
                obs = args.Stream.Observable.Map(WrapSelectForDisposal(args.Selector));
            else
                obs = args.Stream.Observable.Map(WrapSelectIndexForDisposal(args.IndexSelector));
            if (args.ExcludeNull)
                obs = obs.Filter(i => i != null);
            return base.CreateUnsortedStream(obs);
        }
    }
}
