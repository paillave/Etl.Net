using System;
using Paillave.Etl.Reactive.Operators;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class AggregateMultiKeyArgs<TIn, TAggrRes, TMultiKey>
    {
        public IStream<TIn> InputStream { get; set; }
        public Func<TAggrRes, TIn, TAggrRes> Aggregate { get; set; }
        public Func<TIn, TMultiKey> GetKeys { get; set; }
        public Func<TIn, TAggrRes> CreateEmptyAggregation { get; set; }
    }
    public class AggregateMultiKeyStreamNode<TIn, TAggrRes, TMultiKey>(string name, AggregateMultiKeyArgs<TIn, TAggrRes, TMultiKey> args) : StreamNodeBase<AggregationResult<TIn, TMultiKey, TAggrRes>, IStream<AggregationResult<TIn, TMultiKey, TAggrRes>>, AggregateMultiKeyArgs<TIn, TAggrRes, TMultiKey>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

        protected override IStream<AggregationResult<TIn, TMultiKey, TAggrRes>> CreateOutputStream(AggregateMultiKeyArgs<TIn, TAggrRes, TMultiKey> args)
        {
            var keyProcessor = GroupProcessor.Create(args.GetKeys);
            var observableOut = args.InputStream.Observable.Do(keyProcessor.ProcessRow).Last().MultiMap<TIn, AggregationResult<TIn, TMultiKey, TAggrRes>>((i, pushValue) =>
            {
                foreach (var group in keyProcessor.GetGroups())
                {
                    TIn first = default;
                    TAggrRes aggrRes = default;
                    foreach (var item in group.Value)
                    {
                        if (first != null)
                        {
                            first = item;
                            aggrRes = args.CreateEmptyAggregation(first);
                        }
                        aggrRes = args.Aggregate(aggrRes, item);
                    }
                    pushValue(new AggregationResult<TIn, TMultiKey, TAggrRes>
                    {
                        Aggregation = aggrRes,
                        FirstValue = first,
                        Key = group.Key
                    });
                }
            });
            return CreateUnsortedStream(observableOut);
        }
    }
}
