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
    public static partial class UnionEx
    {
        public static IStream<I> Union<I>(this IStream<I> stream, string name, IStream<I> inputStream2)
        {
            return new UnionStreamNode<I>(name, new UnionArgs<I>
            {
                Stream1 = stream,
                Stream2 = inputStream2
            }).Output;
        }

        public static IStream<TOut> Union<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TOut> resultSelectorLeft, Func<TIn2, TOut> resultSelectorRight)
        {
            return new UnionStreamNode<TIn1, TIn2, TOut>(name, new UnionArgs<TIn1, TIn2, TOut>
            {
                Stream1 = stream,
                Stream2 = inputStream2,
                ResultSelectorLeft = resultSelectorLeft,
                ResultSelectorRight = resultSelectorRight
            }).Output;
        }

        public static IStream<TOut> Union<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
        {
            return new UnionStreamNode<TIn1, TIn2, TOut>(name, new UnionArgs<TIn1, TIn2, TOut>
            {
                Stream1 = stream,
                Stream2 = inputStream2,
                FullResultSelectorLeft = resultSelector
            }).Output;
        }

        public static IStream<TOut> Union<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelectorLeft, Func<TIn1, TIn2, TOut> resultSelectorRight)
        {
            return new UnionStreamNode<TIn1, TIn2, TOut>(name, new UnionArgs<TIn1, TIn2, TOut>
            {
                Stream1 = stream,
                Stream2 = inputStream2,
                FullResultSelectorLeft = resultSelectorLeft,
                FullResultSelectorRight = resultSelectorRight,
            }).Output;
        }

        public static IStream<TOut> Union<TIn, TOut>(this IStream<TIn> stream, string name, params Func<ISingleStream<TIn>, IStream<TOut>>[] subProcesses)
        {
            return new ToSubProcessesStreamNode<TIn, TOut>(name, new ToSubProcessesArgs<TIn, TOut>
            {
                Stream = stream,
                NoParallelisation = false,
                SubProcesses = subProcesses
            }).Output;
        }
    }
}