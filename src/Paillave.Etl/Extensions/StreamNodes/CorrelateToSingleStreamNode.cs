using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class CorrelateArgs<TInLeft, TInRight, TOut>
    {
        public IStream<Correlated<TInLeft>> LeftInputStream { get; set; }
        public IStream<Correlated<TInRight>> RightInputStream { get; set; }
        public Func<TInLeft, TInRight, TOut> ResultSelector { get; set; }
    }
    public class CorrelateToSingleStreamNode<TInLeft, TInRight, TOut>(string name, CorrelateArgs<TInLeft, TInRight, TOut> args) : StreamNodeBase<Correlated<TOut>, IStream<Correlated<TOut>>, CorrelateArgs<TInLeft, TInRight, TOut>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<Correlated<TOut>> CreateOutputStream(CorrelateArgs<TInLeft, TInRight, TOut> args)
        {
            var rightDicoS = args.RightInputStream.Observable
                .ToList()
                .Map(l => l
                    .SelectMany(i => i.CorrelationKeys.Select(ck => new
                    {
                        CorrelationKey = ck,
                        Value = i
                    }))
                    .GroupBy(i => i.CorrelationKey)
                    .ToDictionary(i => i.Key, i => i.Select(v => v.Value.Row).ToList()));
            var matchingS = args.LeftInputStream.Observable
                .CombineWithLatest(rightDicoS, (l, rl) =>
                {
                    var element = this.HandleMatching(l.CorrelationKeys, rl).FirstOrDefault();
                    // if (elements.Count > 1)
                    // {
                    //     throw new Exception("Several elements are matching the correlation whereas only one should match.");
                    // }
                    return new
                    {
                        Left = l,
                        Right = element//s.FirstOrDefault()
                    };
                }, true);
            return base.CreateUnsortedStream(matchingS.Map(i => new Correlated<TOut> { Row = args.ResultSelector(i.Left.Row, i.Right), CorrelationKeys = i.Left.CorrelationKeys }));
        }
        private IEnumerable<TInRight> HandleMatching(HashSet<Guid> inputCorrelationKeys, Dictionary<Guid, List<TInRight>> rl)
        {
            foreach (var inputCorrelationKey in inputCorrelationKeys)
                if (rl.TryGetValue(inputCorrelationKey, out var values))
                    foreach (var value in values)
                        yield return value;
        }
    }
    public class CorrelateToManyStreamNode<TInLeft, TInRight, TOut> : StreamNodeBase<Correlated<TOut>, IStream<Correlated<TOut>>, CorrelateArgs<TInLeft, TInRight, TOut>>
    {
        public CorrelateToManyStreamNode(string name, CorrelateArgs<TInLeft, TInRight, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override IStream<Correlated<TOut>> CreateOutputStream(CorrelateArgs<TInLeft, TInRight, TOut> args)
        {
            var rightDicoS = args.RightInputStream.Observable
                .ToList()
                .Map(l => l
                    .SelectMany(i => i.CorrelationKeys.Select(ck => new
                    {
                        CorrelationKey = ck,
                        Value = i
                    }))
                    .GroupBy(i => i.CorrelationKey)
                    .ToDictionary(i => i.Key, i => i.Select(v => v.Value.Row).ToList()));
            var matchingS = args.LeftInputStream.Observable
                .CombineWithLatest(rightDicoS, (l, rl) => this
                    .HandleMatching(l.CorrelationKeys, rl)
                    .Select(i => new Correlated<TOut>
                    {
                        CorrelationKeys = l.CorrelationKeys,
                        Row = args.ResultSelector(l.Row, i)
                    })
                    .ToList(), true);
            return base.CreateUnsortedStream(matchingS.MultiMap((List<Correlated<TOut>> i, Action<Correlated<TOut>> push) => i.ForEach(push)));
        }
        private IEnumerable<TInRight> HandleMatching(HashSet<Guid> inputCorrelationKeys, Dictionary<Guid, List<TInRight>> rl)
        {
            foreach (var inputCorrelationKey in inputCorrelationKeys)
                if (rl.TryGetValue(inputCorrelationKey, out var values))
                    foreach (var value in values)
                        yield return value;
        }
    }
}
