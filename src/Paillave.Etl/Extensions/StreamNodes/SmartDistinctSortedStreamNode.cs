using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class SmartDistinctSortedArgs<TIn, TSortingKey>
    {
        public ISortedStream<TIn, TSortingKey> InputStream { get; set; }
    }
    public class SmartDistinctSortedStreamNode<TIn, TSortingKey> : StreamNodeBase<TIn, IKeyedStream<TIn, TSortingKey>, SmartDistinctSortedArgs<TIn, TSortingKey>>
    {
        public SmartDistinctSortedStreamNode(string name, SmartDistinctSortedArgs<TIn, TSortingKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IKeyedStream<TIn, TSortingKey> CreateOutputStream(SmartDistinctSortedArgs<TIn, TSortingKey> args)
        {
            return base.CreateKeyedStream(
                args.InputStream.Observable.AggregateGrouped(
                    i => i,
                    args.InputStream.SortDefinition.GetKey,
                    (aggr, input) => ObjectMerger.MergeNotNull(aggr, input),
                    (input, key, aggr) => aggr
                ),
                args.InputStream.SortDefinition
            );
        }
    }
    public class SmartDistinctCorrelatedSortedArgs<TIn, TSortingKey>
    {
        public ISortedStream<Correlated<TIn>, TSortingKey> InputStream { get; set; }
    }
    public class SmartDistinctCorrelatedSortedStreamNode<TIn, TSortingKey> : StreamNodeBase<Correlated<TIn>, IKeyedStream<Correlated<TIn>, TSortingKey>, SmartDistinctCorrelatedSortedArgs<TIn, TSortingKey>>
    {
        public SmartDistinctCorrelatedSortedStreamNode(string name, SmartDistinctCorrelatedSortedArgs<TIn, TSortingKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IKeyedStream<Correlated<TIn>, TSortingKey> CreateOutputStream(SmartDistinctCorrelatedSortedArgs<TIn, TSortingKey> args)
        {
            return base.CreateKeyedStream(
                args.InputStream.Observable.AggregateGrouped(
                    i => new Correlated<TIn>
                    {
                        CorrelationKeys = new HashSet<Guid>()
                    },
                    args.InputStream.SortDefinition.GetKey,
                    (aggr, input) =>
                    {
                        aggr.Row = ObjectMerger.MergeNotNull(aggr.Row, input.Row);
                        aggr.CorrelationKeys.UnionWith(input.CorrelationKeys);
                        return aggr;
                    },
                    (input, key, aggr) => aggr
                ),
                args.InputStream.SortDefinition);
        }
    }
}
