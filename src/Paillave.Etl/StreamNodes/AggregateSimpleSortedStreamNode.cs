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
    public class AggregateSimpleSortedArgs<TIn, TAggrRes, TKey>
    {
        public ISortedStream<TIn, TKey> InputStream { get; set; }
        public Expression<Func<TIn, TAggrRes>> AggregationDescriptor;
    }
    public class AggregateSimpleSortedStreamNode<TIn, TAggrRes, TKey> : StreamNodeBase<AggregationResult<TIn, TKey, TAggrRes>, ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey>, AggregateSimpleSortedArgs<TIn, TAggrRes, TKey>>
    {
        public AggregateSimpleSortedStreamNode(string name, AggregateSimpleSortedArgs<TIn, TAggrRes, TKey> args) : base(name, args)
        {
        }

        protected override ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey> CreateOutputStream(AggregateSimpleSortedArgs<TIn, TAggrRes, TKey> args)
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
