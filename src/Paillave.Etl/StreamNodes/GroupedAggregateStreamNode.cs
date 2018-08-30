using System;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System.Collections.Generic;
using Paillave.RxPush.Operators;
using Paillave.RxPush.Core;
using System.Linq;

namespace Paillave.Etl.StreamNodes
{
    public class AggregateGroupedArgs<TIn, TAggr, TKey>
    {
        public ISortedStream<TIn,TKey> InputStream { get; set; }
        public Func<TAggr, TIn, TAggr> Aggregate { get; set; }
        public Func<TAggr> CreateEmptyAggregation { get; set; }
    }
    public class AggregateSortedStreamNode<TIn, TAggr, TKey> : StreamNodeBase<KeyValuePair<TKey, TAggr>, IStream<KeyValuePair<TKey, TAggr>>, AggregateGroupedArgs<TIn, TAggr, TKey>>
    {
        public AggregateSortedStreamNode(string name, AggregateGroupedArgs<TIn, TAggr, TKey> args) : base(name, args)
        {
        }

        protected override IStream<KeyValuePair<TKey, TAggr>> CreateOutputStream(AggregateGroupedArgs<TIn, TAggr, TKey> args)
        {
            throw new NotImplementedException();
            // return CreateStream(args.InputStream.Observable.AggregateGrouped(args.CreateEmptyAggregation,  args.Aggregate).Map(i => new KeyValuePair<TKey, TAggr>(args.GetKey(i.Key), i.Value)));
        }
    }
}
