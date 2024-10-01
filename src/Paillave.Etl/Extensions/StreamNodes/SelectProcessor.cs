using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core
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
        /// Transformation to apply on an occurrence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Output value</returns>
        TOut ProcessRow(TIn value);
    }
    /// <summary>
    /// Implementation of the simplest select transformation
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    /// <remarks>
    /// Builds the processor giving the process to be applied at any occurrence as a parameter
    /// </remarks>
    /// <param name="selector">Delegate describing the tranformation</param>
    public class SimpleSelectProcessor<TIn, TOut>(Func<TIn, TOut> selector) : ISelectProcessor<TIn, TOut>
    {
        private Func<TIn, TOut> _selector = selector;

        /// <summary>
        /// Transformation to apply on an occurrence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Output value</returns>
        public TOut ProcessRow(TIn value) => _selector(value);
    }
    /// <summary>
    /// Implementation of a select transformation adding a sequence for a given keyset
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    /// <remarks>
    /// Builds the processor giving the process to be applied at any occurrence as a parameter
    /// </remarks>
    /// <param name="selector">Delegate describing the tranformation</param>
    public class SelectWithSequenceProcessor<TIn, TKey, TOut>(Func<TIn, int, TOut> selector, Func<TIn, TKey> keySelector) : ISelectProcessor<TIn, TOut>
    {
        private Dictionary<TKey, int> _sequences = new Dictionary<TKey, int>();
        private Func<TIn, int, TOut> _selector = selector;
        private Func<TIn, TKey> _keySelector = keySelector;

        /// <summary>
        /// Transformation to apply on an occurrence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Output value</returns>
        public TOut ProcessRow(TIn value)
        {
            int sequence = 0;
            var key = _keySelector(value);
            _sequences.TryGetValue(key, out sequence);
            _sequences[key] = ++sequence;
            return _selector(value, sequence);
        }
    }
    /// <summary>
    /// Implementation of a select transformation with a context instance
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    /// <typeparam name="TCtx">Context type</typeparam>
    /// <remarks>
    /// Builds the processor giving the process to be applied at any occurrence as a parameter and the initial value of the context
    /// </remarks>
    /// <param name="selector">Delegate describing the tranformation depending on a context that can be updated if necessary</param>
    /// <param name="initialContext">Initial value of the context</param>
    public class ContextSelectProcessor<TIn, TOut, TCtx>(Func<TIn, TCtx, Action<TCtx>, TOut> selector, TCtx initialContext) : ISelectProcessor<TIn, TOut>
    {
        private Func<TIn, TCtx, Action<TCtx>, TOut> _selector = selector;
        private TCtx _context = initialContext;

        /// <summary>
        /// Transformation to apply on an occurrence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <returns>Output value</returns>
        public TOut ProcessRow(TIn value) => _selector(value, _context, SetContext);
        private void SetContext(TCtx newContext) => _context = newContext;
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
        /// Transformation to apply on an occurrence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="index">Occurrence index</param>
        /// <returns>Output value</returns>
        TOut ProcessRow(TIn value, int index);
    }
    /// <summary>
    /// Implementation of a select transformation using occurrence index
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    /// <remarks>
    /// Builds the processor giving the process to be applied at any occurrence using index occurrence as a parameter
    /// </remarks>
    /// <param name="selector">Delegate describing the tranformation using the occurrence index</param>
    public class SimpleSelectWithIndexProcessor<TIn, TOut>(Func<TIn, int, TOut> selector) : ISelectWithIndexProcessor<TIn, TOut>
    {
        private Func<TIn, int, TOut> _selector = selector;

        /// <summary>
        /// Transformation to apply on an occurrence of an element of the stream
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="index">Occurrence index</param>
        /// <returns>Output value</returns>
        public TOut ProcessRow(TIn value, int index) => _selector(value, index);
    }
    /// <summary>
    /// Implementation of a select transformation with a context instance using occurrence index
    /// </summary>
    /// <typeparam name="TIn">Input type</typeparam>
    /// <typeparam name="TOut">Outout type</typeparam>
    /// <typeparam name="TCtx">Context type</typeparam>
    /// <remarks>
    /// Builds the processor giving the process to be applied at any occurrence as a parameter and the initial value of the context
    /// </remarks>
    /// <param name="selector">Delegate describing the tranformation depending on the occurrence index and on a context that can be updated if necessary</param>
    /// <param name="initialContext">Initial value of the context</param>
    public class ContextSelectWithIndexProcessor<TIn, TOut, TCtx>(Func<TIn, int, TCtx, Action<TCtx>, TOut> selector, TCtx initialContext) : ISelectWithIndexProcessor<TIn, TOut>
    {
        private Func<TIn, int, TCtx, Action<TCtx>, TOut> _selector = selector;
        private TCtx _context = initialContext;

        /// <summary>
        /// Transformation to apply on an occurrence of an element of the stream using the occurrence index
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="index">Occurrence index</param>
        /// <returns>Output value</returns>
        public TOut ProcessRow(TIn value, int index) => _selector(value, index, _context, SetContext);
        private void SetContext(TCtx newContext) => _context = newContext;
    }
    #endregion
}
