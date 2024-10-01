using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class DistinctSortedArgs<TIn, TSortingKey>
    {
        public ISortedStream<TIn, TSortingKey> InputStream { get; set; }
    }
    public class DistinctSortedStreamNode<TIn, TSortingKey>(string name, DistinctSortedArgs<TIn, TSortingKey> args) : StreamNodeBase<TIn, IKeyedStream<TIn, TSortingKey>, DistinctSortedArgs<TIn, TSortingKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IKeyedStream<TIn, TSortingKey> CreateOutputStream(DistinctSortedArgs<TIn, TSortingKey> args) => 
            base.CreateKeyedStream(args.InputStream.Observable.DistinctUntilChanged(args.InputStream.SortDefinition), args.InputStream.SortDefinition);
    }


    public class DistinctCorrelatedSortedArgs<TIn, TSortingKey>
    {
        public ISortedStream<Correlated<TIn>, TSortingKey> InputStream { get; set; }
    }
    public class DistinctCorrelatedSortedStreamNode<TIn, TSortingKey>(string name, DistinctCorrelatedSortedArgs<TIn, TSortingKey> args) : StreamNodeBase<Correlated<TIn>, IKeyedStream<Correlated<TIn>, TSortingKey>, DistinctCorrelatedSortedArgs<TIn, TSortingKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IKeyedStream<Correlated<TIn>, TSortingKey> CreateOutputStream(DistinctCorrelatedSortedArgs<TIn, TSortingKey> args) => 
            base.CreateKeyedStream(
                args.InputStream.Observable.AggregateGrouped(
                    i => new HashSet<Guid>(),
                    args.InputStream.SortDefinition, (a, i) =>
                    {
                        a.UnionWith(i.CorrelationKeys);
                        return a;
                    },
                    (i, a) => new Correlated<TIn> { Row = i.Row, CorrelationKeys = a }), args.InputStream.SortDefinition);
    }
}
