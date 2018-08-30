using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System.Collections.Generic;
using Paillave.RxPush.Operators;
using Paillave.RxPush.Core;
using System.Linq;

namespace Paillave.Etl.StreamNodes
{
    public class AggregateGroupedArgs<TIn, TAggr, TKey, TOut>
    {
        public ISortedStream<TIn, TKey> InputStream { get; set; }
        public Func<TAggr, TIn, TAggr> Aggregate { get; set; }
        public Func<TIn, TAggr> CreateEmptyAggregation { get; set; }
        public Func<TIn, TKey, TAggr, TOut> ResultSelector { get; set; }
    }
    public class AggregateSortedStreamNode<TIn, TAggr, TKey, TOut> : StreamNodeBase<TOut, IStream<TOut>, AggregateGroupedArgs<TIn, TAggr, TKey, TOut>>
    {
        public AggregateSortedStreamNode(string name, AggregateGroupedArgs<TIn, TAggr, TKey, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(AggregateGroupedArgs<TIn, TAggr, TKey, TOut> args)
        {
            return CreateUnsortedStream(args.InputStream.Observable.AggregateGrouped(args.CreateEmptyAggregation, args.InputStream.SortDefinition, args.Aggregate, (i, a) => args.ResultSelector(i, args.InputStream.SortDefinition.GetKey(i), a)));
        }
    }
}
