using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System.Collections.Generic;
using Paillave.RxPush.Operators;

namespace Paillave.Etl.StreamNodes
{
    public class AggregateArgs<TIn, TAggr, TKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TAggr, TIn, TAggr> Aggregate { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<TAggr> CreateEmptyAggregation { get; set; }
    }
    public class AggregateStreamNode<TIn, TAggr, TKey> : StreamNodeBase<KeyValuePair<TKey, TAggr>, IStream<KeyValuePair<TKey, TAggr>>, AggregateArgs<TIn, TAggr, TKey>>
    {
        public AggregateStreamNode(string name, AggregateArgs<TIn, TAggr, TKey> args) : base(name, args)
        {
        }

        protected override IStream<KeyValuePair<TKey, TAggr>> CreateOutputStream(AggregateArgs<TIn, TAggr, TKey> args)
        {
            return CreateUnsortedStream(args.InputStream.Observable.Aggregate(args.CreateEmptyAggregation, args.GetKey, args.Aggregate));
        }
    }
}
