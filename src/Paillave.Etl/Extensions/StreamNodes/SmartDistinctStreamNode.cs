using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class SmartDistinctArgs<TIn, TGroupingKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TIn, TGroupingKey> GetGroupingKey { get; set; }
    }
    public class SmartDistinctStreamNode<TIn, TGroupingKey>(string name, SmartDistinctArgs<TIn, TGroupingKey> args) : StreamNodeBase<TIn, IStream<TIn>, SmartDistinctArgs<TIn, TGroupingKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<TIn> CreateOutputStream(SmartDistinctArgs<TIn, TGroupingKey> args) =>
            base.CreateUnsortedStream(args.InputStream.Observable.Aggregate(i => i, args.GetGroupingKey, (aggr, input) => ObjectMerger.MergeNotNull(aggr, input), (input, key, aggr) => aggr));
    }
    public class SmartDistinctCorrelatedArgs<TIn, TGroupingKey>
    {
        public IStream<Correlated<TIn>> InputStream { get; set; }
        public Func<TIn, TGroupingKey> GetGroupingKey { get; set; }
    }
    public class SmartDistinctCorrelatedStreamNode<TIn, TGroupingKey>(string name, SmartDistinctCorrelatedArgs<TIn, TGroupingKey> args) : StreamNodeBase<Correlated<TIn>, IStream<Correlated<TIn>>, SmartDistinctCorrelatedArgs<TIn, TGroupingKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<Correlated<TIn>> CreateOutputStream(SmartDistinctCorrelatedArgs<TIn, TGroupingKey> args) =>
             base.CreateUnsortedStream(args.InputStream.Observable.Aggregate(
                i => new Correlated<TIn> { CorrelationKeys = new HashSet<Guid>() },
                i => args.GetGroupingKey(i.Row),
                (aggr, input) =>
                {
                    aggr.Row = ObjectMerger.MergeNotNull(aggr.Row, input.Row);
                    aggr.CorrelationKeys.UnionWith(input.CorrelationKeys);
                    return aggr;
                },
                (input, key, aggr) => aggr));

    }
}
