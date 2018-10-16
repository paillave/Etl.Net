using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    #region Simple select
    public class SelectArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public ISelectProcessor<TIn, TOut> Processor { get; set; }
        public bool ExcludeNull { get; set; }
    }
    public class SelectStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, SelectArgs<TIn, TOut>>
    {
        public SelectStreamNode(string name, SelectArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(SelectArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.Map(WrapSelectForDisposal<TIn, TOut>(args.Processor.ProcessRow));
            if (args.ExcludeNull)
                obs = obs.Filter(i => i != null);
            return base.CreateUnsortedStream(obs);
        }
    }
    #endregion

    #region Select with index
    public class SelectWithIndexArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public ISelectWithIndexProcessor<TIn, TOut> Processor { get; set; }
        public bool ExcludeNull { get; set; }
    }
    public class SelectWithIndexStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, SelectWithIndexArgs<TIn, TOut>>
    {
        public SelectWithIndexStreamNode(string name, SelectWithIndexArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(SelectWithIndexArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.Map(WrapSelectIndexForDisposal<TIn, TOut>(args.Processor.ProcessRow));
            if (args.ExcludeNull)
                obs = obs.Filter(i => i != null);
            return base.CreateUnsortedStream(obs);
        }
    }
    #endregion

    #region Simple Single select
    public class SelectSingleArgs<TIn, TOut>
    {
        public ISingleStream<TIn> Stream { get; set; }
        public ISelectProcessor<TIn, TOut> Processor { get; set; }
        public bool ExcludeNull { get; set; }
    }
    public class SelectSingleStreamNode<TIn, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, SelectSingleArgs<TIn, TOut>>
    {
        public SelectSingleStreamNode(string name, SelectSingleArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override ISingleStream<TOut> CreateOutputStream(SelectSingleArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.Map(WrapSelectForDisposal<TIn, TOut>(args.Processor.ProcessRow));
            if (args.ExcludeNull)
                obs = obs.Filter(i => i != null);
            return base.CreateSingleStream(obs);
        }
    }
    #endregion

    #region Select Single with index
    public class SelectSingleWithIndexArgs<TIn, TOut>
    {
        public ISingleStream<TIn> Stream { get; set; }
        public ISelectWithIndexProcessor<TIn, TOut> Processor { get; set; }
        public bool ExcludeNull { get; set; }
    }
    public class SelectSingleWithIndexStreamNode<TIn, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, SelectSingleWithIndexArgs<TIn, TOut>>
    {
        public SelectSingleWithIndexStreamNode(string name, SelectSingleWithIndexArgs<TIn, TOut> args) : base(name, args)
        {
        }

        protected override ISingleStream<TOut> CreateOutputStream(SelectSingleWithIndexArgs<TIn, TOut> args)
        {
            IPushObservable<TOut> obs = args.Stream.Observable.Map(WrapSelectIndexForDisposal<TIn, TOut>(args.Processor.ProcessRow));
            if (args.ExcludeNull)
                obs = obs.Filter(i => i != null);
            return base.CreateSingleStream(obs);
        }
    }
    #endregion
}
