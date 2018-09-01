using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System.Collections.Generic;
using Paillave.RxPush.Operators;
using System.Linq.Expressions;
using Paillave.Etl.Core.Aggregation;

namespace Paillave.Etl.StreamNodes
{
    public class PivotArgs<TIn, TAggrRes, TKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Expression<Func<TIn, TAggrRes>> AggregationDescriptor;
    }
    public class PivotStreamNode<TIn, TAggrRes, TKey> : StreamNodeBase<AggregationResult<TIn, TKey, TAggrRes>, IStream<AggregationResult<TIn, TKey, TAggrRes>>, PivotArgs<TIn, TAggrRes, TKey>>
    {
        public PivotStreamNode(string name, PivotArgs<TIn, TAggrRes, TKey> args) : base(name, args)
        {
        }

        protected override IStream<AggregationResult<TIn, TKey, TAggrRes>> CreateOutputStream(PivotArgs<TIn, TAggrRes, TKey> args)
        {
            var aggregationProcessor = new AggregationProcessor<TIn, TAggrRes>(args.AggregationDescriptor);
            return CreateUnsortedStream(
                args.InputStream.Observable.Aggregate(
                    aggregationProcessor.CreateAggregators,
                    args.GetKey,
                    aggregationProcessor.Aggregate, (i, k, a) => new AggregationResult<TIn, TKey, TAggrRes>
                    {
                        Aggregation = aggregationProcessor.CreateInstance(a),
                        FirstValue = i,
                        Key = k
                    }));
        }
    }
}
