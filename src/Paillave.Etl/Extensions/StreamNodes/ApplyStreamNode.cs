using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core
{
    public class ApplyArgs<TInMain, TInToApply, TOut>
    {
        public IStream<TInMain> MainStream { get; set; }
        public ISingleStream<TInToApply> StreamToApply { get; set; }
        public Func<TInMain, TInToApply, TOut> Selector { get; set; }
        public Func<TInMain, TInToApply, int, TOut> IndexSelector { get; set; }
        public bool ExcludeNull { get; set; }
        public bool WithNoDispose { get; set; }
    }
    public class ApplySingleArgs<TInMain, TInToApply, TOut>
    {
        public ISingleStream<TInMain> MainStream { get; set; }
        public ISingleStream<TInToApply> StreamToApply { get; set; }
        public Func<TInMain, TInToApply, TOut> Selector { get; set; }
        public Func<TInMain, TInToApply, int, TOut> IndexSelector { get; set; }
        public bool ExcludeNull { get; set; }
        public bool WithNoDispose { get; set; }
    }
    public class ApplySingleStreamNode<TInMain, TInToApply, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, ApplySingleArgs<TInMain, TInToApply, TOut>>
    {
        public ApplySingleStreamNode(string name, ApplySingleArgs<TInMain, TInToApply, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISingleStream<TOut> CreateOutputStream(ApplySingleArgs<TInMain, TInToApply, TOut> args)
        {
            IPushObservable<TOut> obs;
            if (args.IndexSelector == null)
                obs = args.MainStream.Observable.CombineWithLatest(args.StreamToApply.Observable.First(), WrapSelectForDisposal(args.Selector, args.WithNoDispose), true);
            else
                obs = args.MainStream.Observable.Map((e, i) => new IndexedObject<TInMain>(i, e)).CombineWithLatest(args.StreamToApply.Observable.First(), WrapSelectIndexObjectForDisposal(args.IndexSelector, args.WithNoDispose), true);
            if (args.ExcludeNull)
                obs = obs.Filter(i => i != null);
            return base.CreateSingleStream(obs);
        }
    }

    public class ApplyStreamNode<TInMain, TInToApply, TOut> : StreamNodeBase<TOut, IStream<TOut>, ApplyArgs<TInMain, TInToApply, TOut>>
    {
        public ApplyStreamNode(string name, ApplyArgs<TInMain, TInToApply, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(ApplyArgs<TInMain, TInToApply, TOut> args)
        {
            IPushObservable<TOut> obs;
            if (args.IndexSelector == null)
                obs = args.MainStream.Observable.CombineWithLatest(args.StreamToApply.Observable.First(), WrapSelectForDisposal(args.Selector, args.WithNoDispose), true);
            else
                obs = args.MainStream.Observable.Map((e, i) => new IndexedObject<TInMain>(i, e)).CombineWithLatest(args.StreamToApply.Observable.First(), WrapSelectIndexObjectForDisposal(args.IndexSelector, args.WithNoDispose), true);
            if (args.ExcludeNull)
                obs = obs.Filter(i => i != null);
            return base.CreateUnsortedStream(obs);
        }
    }
}
