using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System.Collections.Generic;
using Paillave.RxPush.Operators;
using Paillave.RxPush.Core;
using System.Linq;

namespace Paillave.Etl.StreamNodes
{
    public class AggregateSortedArgs<TIn, TAggrRes, TKey>
    {
        public ISortedStream<TIn, TKey> InputStream { get; set; }
        public Func<TAggrRes, TIn, TAggrRes> Aggregate { get; set; }
        public Func<TIn, TAggrRes> CreateEmptyAggregation { get; set; }
    }
    public class AggregateSortedStreamNode<TIn, TAggrRes, TKey> : StreamNodeBase<AggregationResult<TIn, TKey, TAggrRes>, ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey>, AggregateSortedArgs<TIn, TAggrRes, TKey>>
    {
        public AggregateSortedStreamNode(string name, AggregateSortedArgs<TIn, TAggrRes, TKey> args) : base(name, args)
        {
        }

        protected override ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey> CreateOutputStream(AggregateSortedArgs<TIn, TAggrRes, TKey> args)
        {
            return CreateSortedStream(
                args.InputStream.Observable.AggregateGrouped(
                    args.CreateEmptyAggregation,
                    args.InputStream.SortDefinition,
                    args.Aggregate,
                    (i, a) => new AggregationResult<TIn, TKey, TAggrRes>
                    {
                        FirstValue = i,
                        Aggregation = a,
                        Key = args.InputStream.SortDefinition.GetKey(i)
                    }),
                new SortDefinition<AggregationResult<TIn, TKey, TAggrRes>, TKey>(i => i.Key, args.InputStream.SortDefinition.KeyPosition));
        }
    }
}
