using Paillave.Etl.Reactive.Operators;
using System;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Core
{
    public class UnionArgs<TIn>
    {
        public IStream<TIn> Stream1 { get; set; }
        public IStream<TIn> Stream2 { get; set; }
    }

    public class UnionStreamNode<TIn> : StreamNodeBase<TIn, IStream<TIn>, UnionArgs<TIn>>
    {
        public UnionStreamNode(string name, UnionArgs<TIn> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TIn> CreateOutputStream(UnionArgs<TIn> args)
        {
            return base.CreateUnsortedStream(args.Stream1.Observable.Merge(args.Stream2.Observable));
        }
    }

    public class UnionArgs<TIn1, TIn2, TOut>
    {
        public IStream<TIn1> Stream1 { get; set; }
        public IStream<TIn2> Stream2 { get; set; }
        public Func<TIn1, TOut> ResultSelectorLeft { get; set; }
        public Func<TIn2, TOut> ResultSelectorRight { get; set; }
        public Func<TIn1, TIn2, TOut> FullResultSelectorLeft { get; set; }
        public Func<TIn1, TIn2, TOut> FullResultSelectorRight { get; set; }
    }

    public class UnionStreamNode<TIn1, TIn2, TOut> : StreamNodeBase<TOut, IStream<TOut>, UnionArgs<TIn1, TIn2, TOut>>
    {
        public UnionStreamNode(string name, UnionArgs<TIn1, TIn2, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(UnionArgs<TIn1, TIn2, TOut> args)
        {
            IPushObservable<TOut> obs;
            if (args.FullResultSelectorLeft != null)
            {
                obs = args.Stream1.Observable.Map(i => args.FullResultSelectorLeft(i, default));
                obs = obs.Merge(args.FullResultSelectorRight == null
                    ? args.Stream2.Observable.Map(i => args.FullResultSelectorLeft(default, i))
                    : args.Stream2.Observable.Map(i => args.FullResultSelectorRight(default, i)));
            }
            else
                obs = args.Stream1.Observable.Map(i => args.ResultSelectorLeft(i))
                    .Merge(args.Stream2.Observable.Map(i => args.ResultSelectorRight(i)));

            return base.CreateUnsortedStream(obs);
        }
    }
}