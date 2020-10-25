using System;
using System.Collections.Generic;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.StreamNodes
{
    public class DistinctSortedArgs<TIn, TSortingKey>
    {
        public ISortedStream<TIn, TSortingKey> InputStream { get; set; }
    }
    public class DistinctSortedStreamNode<TIn, TSortingKey> : StreamNodeBase<TIn, IKeyedStream<TIn, TSortingKey>, DistinctSortedArgs<TIn, TSortingKey>>
    {
        public DistinctSortedStreamNode(string name, DistinctSortedArgs<TIn, TSortingKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IKeyedStream<TIn, TSortingKey> CreateOutputStream(DistinctSortedArgs<TIn, TSortingKey> args)
        {
            return base.CreateKeyedStream(args.InputStream.Observable.DistinctUntilChanged(args.InputStream.SortDefinition), args.InputStream.SortDefinition);
        }
    }


    public class DistinctCorrelatedSortedArgs<TIn, TSortingKey>
    {
        public ISortedStream<Correlated<TIn>, TSortingKey> InputStream { get; set; }
    }
    public class DistinctCorrelatedSortedStreamNode<TIn, TSortingKey> : StreamNodeBase<Correlated<TIn>, IKeyedStream<Correlated<TIn>, TSortingKey>, DistinctCorrelatedSortedArgs<TIn, TSortingKey>>
    {
        public DistinctCorrelatedSortedStreamNode(string name, DistinctCorrelatedSortedArgs<TIn, TSortingKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IKeyedStream<Correlated<TIn>, TSortingKey> CreateOutputStream(DistinctCorrelatedSortedArgs<TIn, TSortingKey> args)
        {
            return base.CreateKeyedStream(
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
}
