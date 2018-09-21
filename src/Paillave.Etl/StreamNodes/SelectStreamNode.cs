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
    public interface ISelectProcessor<TIn, TOut>
    {
        TOut ProcessRow(TIn value);
    }
    public class SimpleSelectProcessor<TIn, TOut> : ISelectProcessor<TIn, TOut>
    {
        public Func<TIn, TOut> _selector;
        public SimpleSelectProcessor(Func<TIn, TOut> selector)
        {
            _selector = selector;
        }
        public TOut ProcessRow(TIn value)
        {
            return _selector(value);
        }
    }
    public class ContextSelectProcessor<TIn, TOut, TCtx> : ISelectProcessor<TIn, TOut>
    {
        public Func<TIn, TCtx, Action<TCtx>, TOut> _selector;
        private TCtx _context;
        public ContextSelectProcessor(Func<TIn, TCtx, Action<TCtx>, TOut> selector, TCtx initialContext)
        {
            _selector = selector;
            _context = initialContext;
        }
        public TOut ProcessRow(TIn value)
        {
            return _selector(value, _context, SetContext);
        }
        private void SetContext(TCtx newContext)
        {
            _context = newContext;
        }
    }
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
    public interface ISelectWithIndexProcessor<TIn, TOut>
    {
        TOut ProcessRow(TIn value, int index);
    }
    public class SimpleSelectWithIndexProcessor<TIn, TOut> : ISelectWithIndexProcessor<TIn, TOut>
    {
        public Func<TIn, int, TOut> _selector;
        public SimpleSelectWithIndexProcessor(Func<TIn, int, TOut> selector)
        {
            _selector = selector;
        }
        public TOut ProcessRow(TIn value, int index)
        {
            return _selector(value, index);
        }
    }
    public class ContextSelectWithIndexProcessor<TIn, TOut, TCtx> : ISelectWithIndexProcessor<TIn, TOut>
    {
        public Func<TIn, int, TCtx, Action<TCtx>, TOut> _selector;
        private TCtx _context;
        public ContextSelectWithIndexProcessor(Func<TIn, int, TCtx, Action<TCtx>, TOut> selector, TCtx initialContext)
        {
            _selector = selector;
            _context = initialContext;
        }
        public TOut ProcessRow(TIn value, int index)
        {
            return _selector(value, index, _context, SetContext);
        }
        private void SetContext(TCtx newContext)
        {
            _context = newContext;
        }
    }
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
}
