using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SystemIO = System.IO;

namespace Paillave.Etl.Extensions
{
    /// <summary>
    /// Produce a stream of rows based on an enumerable
    /// </summary>
    public static partial class CrossApplyEnumerableEx
    {
        /// <summary>
        /// Produces a stream of rows based on a enumerable using input stream
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="values">Enumerable that contains values to submit to the output stream</param>
        /// <param name="noParallelisation">If set to true, values won't be issued concurrently, preventing them to be mixed</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <returns>Output stream</returns>
        public static IStream<TOut> CrossApplyEnumerable<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<TOut>> values, bool noParallelisation = false)
            => stream.CrossApply<TIn, TOut>(name, EnumerableValuesProvider.Create(values), noParallelisation);


        /// <summary>
        /// Produces a stream of rows based on a enumerable using input stream
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="values">Enumerable that contains values to submit to the output stream</param>
        /// <param name="noParallelisation">If set to true, values won't be issued concurrently, preventing them to be mixed</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <returns>Output stream</returns>
        public static IStream<Correlated<TOut>> CrossApplyEnumerable<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, IEnumerable<TOut>> values, bool noParallelisation = false)
            => stream.CrossApply<Correlated<TIn>, Correlated<TOut>>(name, new ValueProviderCorrelationWrapper<TIn, TOut>(EnumerableValuesProvider.Create(values)), noParallelisation);
    }
}
