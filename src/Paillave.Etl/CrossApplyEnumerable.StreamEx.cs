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

namespace Paillave.Etl
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
        /// <param name="noParallelisation">if set to true, values won't be issued concurrently, preventing them to be mixed</param>
        /// <typeparam name="TIn">Input type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <returns>Output stream</returns>
        public static IStream<TOut> CrossApplyEnumerable<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<TOut>> values, bool noParallelisation = false)
        {
            return stream.CrossApply(name, new ActionValuesProvider<TIn, TOut>(new ActionValuesProviderArgs<TIn, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = (input, push) =>
                {
                    foreach (var value in values(input))
                        push(value);
                }
            }), i => i, (i, _) => i);
        }
        /// <summary>
        /// Produces a stream of rows based on a enumerable using input stream and resource stream
        /// </summary>
        /// <param name="stream">Input stream</param>
        /// <param name="name">Name of the operation</param>
        /// <param name="streamToApply">The stream that contains the single element that will be applied to each element of the main stream with the result selector</param>
        /// <param name="values">Enumerable that contains values to submit to the output stream</param>
        /// <param name="noParallelisation">if set to true, values won't be issued concurrently, preventing them to be mixed</param>
        /// <typeparam name="TIn1">Input type</typeparam>
        /// <typeparam name="TIn2">Resource type</typeparam>
        /// <typeparam name="TOut">Output type</typeparam>
        /// <returns>Output stream</returns>
        public static IStream<TOut> CrossApplyEnumerable<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, ISingleStream<TIn2> streamToApply, Func<TIn1, TIn2, IEnumerable<TOut>> values, bool noParallelisation = false)
        {
            return stream.CrossApply(name, streamToApply, new ActionResourceValuesProvider<TIn1, TIn2, TOut>(new ActionResourceValuesProviderArgs<TIn1, TIn2, TOut>()
            {
                NoParallelisation = noParallelisation,
                ProduceValues = (input1, input2, push) =>
                {
                    foreach (var value in values(input1, input2))
                        push(value);
                }
            }), (i, _) => i, (i, _, __) => i);
        }
    }
}
