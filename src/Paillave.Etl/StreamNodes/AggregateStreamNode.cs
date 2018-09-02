using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.StreamNodes
{
    public class AggregateArgs<TIn, TAggrRes, TKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TAggrRes, TIn, TAggrRes> Aggregate { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<TIn, TAggrRes> CreateEmptyAggregation { get; set; }
    }
    public class AggregateStreamNode<TIn, TAggrRes, TKey> : StreamNodeBase<AggregationResult<TIn, TKey, TAggrRes>, IStream<AggregationResult<TIn, TKey, TAggrRes>>, AggregateArgs<TIn, TAggrRes, TKey>>
    {
        public AggregateStreamNode(string name, AggregateArgs<TIn, TAggrRes, TKey> args) : base(name, args)
        {
        }

        protected override IStream<AggregationResult<TIn, TKey, TAggrRes>> CreateOutputStream(AggregateArgs<TIn, TAggrRes, TKey> args)
        {
            return CreateUnsortedStream(
                args.InputStream.Observable.Aggregate(
                    args.CreateEmptyAggregation,
                    args.GetKey,
                    args.Aggregate,
                    (i, k, a) => new AggregationResult<TIn, TKey, TAggrRes>
                    {
                        Aggregation = a,
                        FirstValue = i,
                        Key = k
                    }));
        }
    }
}
