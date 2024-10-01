using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class AggregateArgs<TIn, TAggrRes, TKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TAggrRes, TIn, TAggrRes> Aggregate { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<TIn, TAggrRes> CreateEmptyAggregation { get; set; }
    }
    public class AggregateStreamNode<TIn, TAggrRes, TKey>(string name, AggregateArgs<TIn, TAggrRes, TKey> args) : StreamNodeBase<AggregationResult<TIn, TKey, TAggrRes>, IStream<AggregationResult<TIn, TKey, TAggrRes>>, AggregateArgs<TIn, TAggrRes, TKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override IStream<AggregationResult<TIn, TKey, TAggrRes>> CreateOutputStream(AggregateArgs<TIn, TAggrRes, TKey> args)
        {
            return CreateUnsortedStream(
                args.InputStream.Observable.Aggregate(
                    args.CreateEmptyAggregation,
                    args.GetKey,
                    args.Aggregate,
                    (i, k, a) => new AggregationResult<TIn, TKey, TAggrRes>
                    {
                        Aggregation = a,
                        FirstValue = i,
                        Key = k
                    }));
        }
    }
    public class AggregateCorrelatedArgs<TIn, TAggrRes, TKey>
    {
        public IStream<Correlated<TIn>> InputStream { get; set; }
        public Func<TAggrRes, TIn, TAggrRes> Aggregate { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<TIn, TAggrRes> CreateEmptyAggregation { get; set; }
    }
    public class AggregateCorrelatedStreamNode<TIn, TAggrRes, TKey> : StreamNodeBase<Correlated<AggregationResult<TIn, TKey, TAggrRes>>, IStream<Correlated<AggregationResult<TIn, TKey, TAggrRes>>>, AggregateCorrelatedArgs<TIn, TAggrRes, TKey>>
    {
        public AggregateCorrelatedStreamNode(string name, AggregateCorrelatedArgs<TIn, TAggrRes, TKey> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override IStream<Correlated<AggregationResult<TIn, TKey, TAggrRes>>> CreateOutputStream(AggregateCorrelatedArgs<TIn, TAggrRes, TKey> args)
        {
            return CreateUnsortedStream(
                args.InputStream.Observable.Aggregate(
                    i => new Correlated<TAggrRes>
                    {
                        Row = args.CreateEmptyAggregation(i.Row),
                        CorrelationKeys = new HashSet<Guid>()
                    },
                    i => args.GetKey(i.Row),
                    (a, i) =>
                    {
                        a.Row = args.Aggregate(a.Row, i.Row);
                        a.CorrelationKeys.UnionWith(i.CorrelationKeys);
                        return a;
                    },
                    (i, k, a) => new Correlated<AggregationResult<TIn, TKey, TAggrRes>>
                    {
                        CorrelationKeys = a.CorrelationKeys,
                        Row = new AggregationResult<TIn, TKey, TAggrRes>
                        {
                            Aggregation = a.Row,
                            FirstValue = i.Row,
                            Key = k
                        }
                    }));
        }
    }





























    public class AggregateArgs<TIn, TAggrRes>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TAggrRes, TIn, TAggrRes> Aggregate { get; set; }
    }
    public class AggregateStreamNode<TIn, TAggrRes> : StreamNodeBase<TAggrRes, IStream<TAggrRes>, AggregateArgs<TIn, TAggrRes>>
    {
        public AggregateStreamNode(string name, AggregateArgs<TIn, TAggrRes> args) : base(name, args) { }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override IStream<TAggrRes> CreateOutputStream(AggregateArgs<TIn, TAggrRes> args)
        {
            return CreateUnsortedStream(args.InputStream.Observable.Aggregate(args.Aggregate));
        }
    }
}
