using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using System.Linq.Expressions;
using Paillave.Etl.Core.Aggregation;
using Paillave.Etl.Reactive.Core;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class PivotArgs<TIn, TAggrRes, TKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Expression<Func<TIn, TAggrRes>> AggregationDescriptor;
    }
    public class PivotStreamNode<TIn, TAggrRes, TKey>(string name, PivotArgs<TIn, TAggrRes, TKey> args) : StreamNodeBase<AggregationResult<TIn, TKey, TAggrRes>, IStream<AggregationResult<TIn, TKey, TAggrRes>>, PivotArgs<TIn, TAggrRes, TKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<AggregationResult<TIn, TKey, TAggrRes>> CreateOutputStream(PivotArgs<TIn, TAggrRes, TKey> args)
        {
            var aggregationProcessor = new AggregationProcessor<TIn, TIn, TAggrRes, Core.Aggregation.Aggregator<TIn>>(args.AggregationDescriptor);
            return CreateUnsortedStream(
                args.InputStream.Observable.Aggregate(
                    aggregationProcessor.CreateAggregators,
                    args.GetKey,
                    aggregationProcessor.Aggregate, (i, k, a) => new AggregationResult<TIn, TKey, TAggrRes>
                    {
                        Aggregation = aggregationProcessor.CreateInstance(a),
                        FirstValue = i,
                        Key = k
                    }));
        }
    }
    public class PivotSortedArgs<TIn, TAggrRes, TKey>
    {
        public ISortedStream<TIn, TKey> InputStream { get; set; }
        public Expression<Func<TIn, TAggrRes>> AggregationDescriptor;
    }
    public class PivotSortedStreamNode<TIn, TAggrRes, TKey>(string name, PivotSortedArgs<TIn, TAggrRes, TKey> args) : StreamNodeBase<AggregationResult<TIn, TKey, TAggrRes>, ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey>, PivotSortedArgs<TIn, TAggrRes, TKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISortedStream<AggregationResult<TIn, TKey, TAggrRes>, TKey> CreateOutputStream(PivotSortedArgs<TIn, TAggrRes, TKey> args)
        {
            var aggregationProcessor = new AggregationProcessor<TIn, TIn, TAggrRes, Core.Aggregation.Aggregator<TIn>>(args.AggregationDescriptor);
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











    public class PivotCorrelatedArgs<TIn, TAggrRes, TKey>
    {
        public IStream<Correlated<TIn>> InputStream { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Expression<Func<TIn, TAggrRes>> AggregationDescriptor;
    }
    public class PivotCorrelatedStreamNode<TIn, TAggrRes, TKey> : StreamNodeBase<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, IStream<Correlated<AggregationResult<TIn, TKey, TAggrRes>>>, PivotCorrelatedArgs<TIn, TAggrRes, TKey>>
    {
        public PivotCorrelatedStreamNode(string name, PivotCorrelatedArgs<TIn, TAggrRes, TKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<Correlated<AggregationResult<TIn, TKey, TAggrRes>>> CreateOutputStream(PivotCorrelatedArgs<TIn, TAggrRes, TKey> args)
        {
            var aggregationProcessor = new AggregationProcessor<Correlated<TIn>, TIn, TAggrRes, CorrelatedAggregator<TIn>>(args.AggregationDescriptor);
            return CreateUnsortedStream(
                args.InputStream.Observable.Aggregate(
                    i => aggregationProcessor.CreateAggregators(i),
                    i => args.GetKey(i.Row),
                    aggregationProcessor.Aggregate,
                    (i, k, a) =>
                    {
                        HashSet<Guid> correlationKeys = new HashSet<Guid>();
                        a.Values.Select(i => i.CorrelationKeys).ToList().ForEach(correlationKeys.UnionWith);
                        return new Correlated<AggregationResult<TIn, TKey, TAggrRes>>
                        {
                            Row = new AggregationResult<TIn, TKey, TAggrRes>
                            {
                                Aggregation = aggregationProcessor.CreateInstance(a),
                                FirstValue = i.Row,
                                Key = k
                            },
                            CorrelationKeys = correlationKeys
                        };
                    }));
        }
    }
    public class PivotSortedCorrelatedArgs<TIn, TAggrRes, TKey>
    {
        public ISortedStream<Correlated<TIn>, TKey> InputStream { get; set; }
        public Expression<Func<TIn, TAggrRes>> AggregationDescriptor;
    }
    public class PivotSortedCorrelatedStreamNode<TIn, TAggrRes, TKey> : StreamNodeBase<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, ISortedStream<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, TKey>, PivotSortedCorrelatedArgs<TIn, TAggrRes, TKey>>
    {
        public PivotSortedCorrelatedStreamNode(string name, PivotSortedCorrelatedArgs<TIn, TAggrRes, TKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISortedStream<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, TKey> CreateOutputStream(PivotSortedCorrelatedArgs<TIn, TAggrRes, TKey> args)
        {
            var aggregationProcessor = new AggregationProcessor<Correlated<TIn>, TIn, TAggrRes, CorrelatedAggregator<TIn>>(args.AggregationDescriptor);
            return CreateSortedStream(
                args.InputStream.Observable.AggregateGrouped(
                    aggregationProcessor.CreateAggregators,
                    args.InputStream.SortDefinition.GetKey,
                    aggregationProcessor.Aggregate,
                    (i, k, a) =>
                    {
                        HashSet<Guid> correlationKeys = new HashSet<Guid>();
                        a.Values.Select(i => i.CorrelationKeys).ToList().ForEach(correlationKeys.UnionWith);
                        return new Correlated<AggregationResult<TIn, TKey, TAggrRes>>
                        {
                            Row = new AggregationResult<TIn, TKey, TAggrRes>
                            {
                                Aggregation = aggregationProcessor.CreateInstance(a),
                                FirstValue = i.Row,
                                Key = k
                            },
                            CorrelationKeys = correlationKeys
                        };
                    }),
                new SortDefinition<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, TKey>(i => i.Row.Key, args.InputStream.SortDefinition.KeyPosition));
        }
    }
    public class AggregationResult<TIn, TKey, TAggrRes>
    {
        public TIn FirstValue { get; set; }
        public TKey Key { get; set; }
        public TAggrRes Aggregation { get; set; }
    }
}
