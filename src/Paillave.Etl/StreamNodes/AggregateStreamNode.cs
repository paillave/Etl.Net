using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System.Collections.Generic;
using Paillave.RxPush.Operators;

namespace Paillave.Etl.StreamNodes
{
    public class AggregateArgs<TIn, TAggr, TKey, TOut>
    {
        public Func<TIn, TKey, TAggr, TOut> ResultSelector { get; set; }
        public IStream<TIn> InputStream { get; set; }
        public Func<TAggr, TIn, TAggr> Aggregate { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<TIn, TAggr> CreateEmptyAggregation { get; set; }
    }
    public class AggregateStreamNode<TIn, TAggr, TKey,TOut> : StreamNodeBase<TOut, IStream<TOut>, AggregateArgs<TIn, TAggr, TKey,TOut>>
    {
        public AggregateStreamNode(string name, AggregateArgs<TIn, TAggr, TKey,TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(AggregateArgs<TIn, TAggr, TKey,TOut> args)
        {
            return CreateUnsortedStream(args.InputStream.Observable.Aggregate(args.CreateEmptyAggregation, args.GetKey, args.Aggregate, args.ResultSelector));
        }
    }
}
