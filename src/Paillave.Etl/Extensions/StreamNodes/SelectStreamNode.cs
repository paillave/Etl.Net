using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    #region Simple select
    public class SelectArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public ISelectProcessor<TIn, TOut> Processor { get; set; }
        public bool WithNoDispose { get; set; }
    }
    public class SelectStreamNode<TIn, TOut>(string name, SelectArgs<TIn, TOut> args) : StreamNodeBase<TOut, IStream<TOut>, SelectArgs<TIn, TOut>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(SelectArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.Map(WrapSelectForDisposal<TIn, TOut>(args.Processor.ProcessRow, args.WithNoDispose));
            return base.CreateUnsortedStream(obs);
        }
    }
    #endregion

    #region Select with index
    public class SelectWithIndexArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public ISelectWithIndexProcessor<TIn, TOut> Processor { get; set; }
        public bool WithNoDispose { get; set; }
    }
    public class SelectWithIndexStreamNode<TIn, TOut>(string name, SelectWithIndexArgs<TIn, TOut> args) : StreamNodeBase<TOut, IStream<TOut>, SelectWithIndexArgs<TIn, TOut>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(SelectWithIndexArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.Map(WrapSelectIndexForDisposal<TIn, TOut>(args.Processor.ProcessRow, args.WithNoDispose));
            return base.CreateUnsortedStream(obs);
        }
    }
    #endregion

    #region Simple Single select
    public class SelectSingleArgs<TIn, TOut>
    {
        public ISingleStream<TIn> Stream { get; set; }
        public ISelectProcessor<TIn, TOut> Processor { get; set; }
        public bool WithNoDispose { get; set; }
    }
    public class SelectSingleStreamNode<TIn, TOut>(string name, SelectSingleArgs<TIn, TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, SelectSingleArgs<TIn, TOut>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISingleStream<TOut> CreateOutputStream(SelectSingleArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.Map(WrapSelectForDisposal<TIn, TOut>(args.Processor.ProcessRow, args.WithNoDispose));
            return base.CreateSingleStream(obs);
        }
    }
    #endregion
}
