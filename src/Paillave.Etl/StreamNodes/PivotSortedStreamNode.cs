using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System.Collections.Generic;
using Paillave.RxPush.Operators;
using Paillave.RxPush.Core;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.Core.Aggregation;

namespace Paillave.Etl.StreamNodes
{
    public class PivotSortedArgs<TIn, TAggrRes, TKey>
    {
        public ISortedStream<TIn, TKey> InputStream { get; set; }
        public Expression<Func<TIn, TAggrRes>> AggregationDescriptor;
    }
    public class PivotSortedStreamNode<TIn, TAggrRes, TKey> : StreamNodeBase<AggregationResult<TIn, TKey, TAggrRes>, ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey>, PivotSortedArgs<TIn, TAggrRes, TKey>>
    {
        public PivotSortedStreamNode(string name, PivotSortedArgs<TIn, TAggrRes, TKey> args) : base(name, args)
        {
        }

        protected override ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey> CreateOutputStream(PivotSortedArgs<TIn, TAggrRes, TKey> args)
        {
            var aggregationProcessor = new AggregationProcessor<TIn, TAggrRes>(args.AggregationDescriptor);
            return CreateSortedStream(
                args.InputStream.Observable.AggregateGrouped(
                    aggregationProcessor.CreateAggregators,
                    args.InputStream.SortDefinition.GetKey,
                    aggregationProcessor.Aggregate,
                    (i, k, a) => new AggregationResult<TIn, TKey, TAggrRes>
                    {
                        Aggregation = aggregationProcessor.CreateInstance(a),
                        FirstValue = i,
                        Key = k
                    }),
                new SortDefinition<AggregationResult<TIn, TKey, TAggrRes>, TKey>(i => i.Key, args.InputStream.SortDefinition.KeyPosition));
        }
    }
    public class AggregationResult<TIn, TKey, TAggrRes>
    {
        public TIn FirstValue { get; set; }
        public TKey Key { get; set; }
        public TAggrRes Aggregation { get; set; }
    }
}
