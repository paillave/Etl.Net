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
    public static partial class UnionAllEx
    {
        public static IStream<I> UnionAll<I>(this IStream<I> stream, string name, IStream<I> inputStream2)
        {
            return new UnionAllStreamNode<I>(name, new UnionAllArgs<I>
            {
                Stream1 = stream,
                Stream2 = inputStream2
            }).Output;
        }
        public static IStream<TOut> UnionAll<TIn, TOut>(this IStream<TIn> stream, string name, params Func<ISingleStream<TIn>, IStream<TOut>>[] subProcesses)
        {
            return new ToSubProcessesStreamNode<TIn, TOut>(name, new ToSubProcessesArgs<TIn, TOut>
            {
                Stream = stream,
                NoParallelisation = true,
                SubProcesses = subProcesses
            }).Output;
        }
    }
}
