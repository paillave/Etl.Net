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
    /// <summary>
    /// Description of a select transformation. 
    /// The same instance of processor will be used by a select node, so it can contain a whole set of variables a as a context.
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    public interface ISelectProcessor<TIn, TOut>
    {
        /// <summary>
        /// Transformation to apply on an occurence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Output value</returns>
        TOut ProcessRow(TIn value);
    }
    /// <summary>
    /// Implementation of the simpliest select transformation
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    public class SimpleSelectProcessor<TIn, TOut> : ISelectProcessor<TIn, TOut>
    {
        private Func<TIn, TOut> _selector;
        /// <summary>
        /// Builds the processor giving the process to be applied at any occurence as a parameter
        /// </summary>
        /// <param name="selector">Delegate describing the tranformation</param>
        public SimpleSelectProcessor(Func<TIn, TOut> selector)
        {
            _selector = selector;
        }
        /// <summary>
        /// Transformation to apply on an occurence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Output value</returns>
        public TOut ProcessRow(TIn value)
        {
            return _selector(value);
        }
    }
    /// <summary>
    /// Implementation of a select transformation with a context instance
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    /// <typeparam name="TCtx">Context type</typeparam>
    public class ContextSelectProcessor<TIn, TOut, TCtx> : ISelectProcessor<TIn, TOut>
    {
        private Func<TIn, TCtx, Action<TCtx>, TOut> _selector;
        private TCtx _context;
        /// <summary>
        /// Builds the processor giving the process to be applied at any occurence as a parameter and the initial value of the context
        /// </summary>
        /// <param name="selector">Delegate describing the tranformation depending on a context that can be updated if necessary</param>
        /// <param name="initialContext">Initial value of the context</param>
        public ContextSelectProcessor(Func<TIn, TCtx, Action<TCtx>, TOut> selector, TCtx initialContext)
        {
            _selector = selector;
            _context = initialContext;
        }
        /// <summary>
        /// Transformation to apply on an occurence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Output value</returns>
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
    /// <summary>
    /// Description of a select transformation depending on occurrence index. 
    /// The same instance of processor will be used by a select node, so it can contain a whole set of variables a as a context.
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    public interface ISelectWithIndexProcessor<TIn, TOut>
    {
        /// <summary>
        /// Transformation to apply on an occurence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="index">Occurence index</param>
        /// <returns>Output value</returns>
        TOut ProcessRow(TIn value, int index);
    }
    /// <summary>
    /// Implementation of a select transformation using occurrence index
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    public class SimpleSelectWithIndexProcessor<TIn, TOut> : ISelectWithIndexProcessor<TIn, TOut>
    {
        private Func<TIn, int, TOut> _selector;
        /// <summary>
        /// Builds the processor giving the process to be applied at any occurence using index occurence as a parameter
        /// </summary>
        /// <param name="selector">Delegate describing the tranformation using the occurrence index</param>
        public SimpleSelectWithIndexProcessor(Func<TIn, int, TOut> selector)
        {
            _selector = selector;
        }
        /// <summary>
        /// Transformation to apply on an occurence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="index">Occurrence index</param>
        /// <returns>Output value</returns>
        public TOut ProcessRow(TIn value, int index)
        {
            return _selector(value, index);
        }
    }
    /// <summary>
    /// Implementation of a select transformation with a context instance using occurrence index
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    /// <typeparam name="TCtx">Context type</typeparam>
    public class ContextSelectWithIndexProcessor<TIn, TOut, TCtx> : ISelectWithIndexProcessor<TIn, TOut>
    {
        private Func<TIn, int, TCtx, Action<TCtx>, TOut> _selector;
        private TCtx _context;
        /// <summary>
        /// Builds the processor giving the process to be applied at any occurence as a parameter and the initial value of the context
        /// </summary>
        /// <param name="selector">Delegate describing the tranformation depending on the occurrence index and on a context that can be updated if necessary</param>
        /// <param name="initialContext">Initial value of the context</param>
        public ContextSelectWithIndexProcessor(Func<TIn, int, TCtx, Action<TCtx>, TOut> selector, TCtx initialContext)
        {
            _selector = selector;
            _context = initialContext;
        }
        /// <summary>
        /// Transformation to apply on an occurence of an element of the stream using the occurence index
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="index">Occurence index</param>
        /// <returns>Output value</returns>
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
