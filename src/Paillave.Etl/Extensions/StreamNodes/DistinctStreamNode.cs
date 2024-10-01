using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public class DistinctArgs<TIn, TGroupingKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TIn, TGroupingKey> GetGroupingKey { get; set; }
    }
    public class DistinctStreamNode<TIn, TGroupingKey>(string name, DistinctArgs<TIn, TGroupingKey> args) : StreamNodeBase<TIn, IStream<TIn>, DistinctArgs<TIn, TGroupingKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override IStream<TIn> CreateOutputStream(DistinctArgs<TIn, TGroupingKey> args) =>
            base.CreateUnsortedStream(args.InputStream.Observable.Distinct(args.GetGroupingKey));
    }
    public class DistinctCorrelatedArgs<TIn, TGroupingKey>
    {
        public IStream<Correlated<TIn>> InputStream { get; set; }
        public Func<TIn, TGroupingKey> GetGroupingKey { get; set; }
    }
    public class DistinctCorrelatedStreamNode<TIn, TGroupingKey>(string name, DistinctCorrelatedArgs<TIn, TGroupingKey> args) : StreamNodeBase<Correlated<TIn>, IStream<Correlated<TIn>>, DistinctCorrelatedArgs<TIn, TGroupingKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override IStream<Correlated<TIn>> CreateOutputStream(DistinctCorrelatedArgs<TIn, TGroupingKey> args) =>
             base.CreateUnsortedStream(args.InputStream.Observable.Aggregate(i => new HashSet<Guid>(), i => args.GetGroupingKey(i.Row), (a, i) =>
            {
                a.UnionWith(i.CorrelationKeys);
                return a;
            }, (i, k, a) => new Correlated<TIn> { Row = i.Row, CorrelationKeys = a }));

    }
}
