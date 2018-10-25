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
    public static partial class CrossApplyEx
    {
        public static IStream<TOut> CrossApply<TIn, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, Action<TValueIn, Action<TValueOut>> valuesProvider, Func<TIn, TValueIn> preProcessValue, Func<TValueOut, TIn, TOut> postProcessValue, bool noParallelisation = false)
        {
            return new CrossApplyStreamNode<TIn, TValueIn, TValueOut, TOut>(name, new CrossApplyArgs<TIn, TValueIn, TValueOut, TOut>
            {
                NoParallelisation = noParallelisation,
                Stream = stream,
                GetValueIn = preProcessValue,
                GetValueOut = postProcessValue,
                ValuesProvider = valuesProvider
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TOut>(this IStream<TIn> stream, string name, Action<TIn, Action<TOut>> valuesProvider, bool noParallelisation = false)
        {
            return new CrossApplyStreamNode<TIn, TIn, TOut, TOut>(name, new CrossApplyArgs<TIn, TIn, TOut, TOut>
            {
                NoParallelisation = noParallelisation,
                Stream = stream,
                GetValueIn = i => i,
                GetValueOut = (i, j) => i,
                ValuesProvider = valuesProvider
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TInToApply, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, ISingleStream<TInToApply> resourceStream, Action<TValueIn, TInToApply, Action<TValueOut>> valuesProvider, Func<TIn, TInToApply, TValueIn> preProcessValue, Func<TValueOut, TIn, TInToApply, TOut> postProcessValue, bool noParallelisation = false)
        {
            return new CrossApplyStreamNode<TIn, TInToApply, TValueIn, TValueOut, TOut>(name, new CrossApplyArgs<TIn, TInToApply, TValueIn, TValueOut, TOut>
            {
                NoParallelisation = noParallelisation,
                MainStream = stream,
                GetValueIn = preProcessValue,
                GetValueOut = postProcessValue,
                ValuesProvider = valuesProvider,
                StreamToApply = resourceStream
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TInToApply, TOut>(this IStream<TIn> stream, string name, ISingleStream<TInToApply> resourceStream, Action<TIn, TInToApply, Action<TOut>> valuesProvider, bool noParallelisation = false)
        {
            return new CrossApplyStreamNode<TIn, TInToApply, TIn, TOut, TOut>(name, new CrossApplyArgs<TIn, TInToApply, TIn, TOut, TOut>
            {
                NoParallelisation = noParallelisation,
                MainStream = stream,
                GetValueIn = (i, j) => i,
                GetValueOut = (i, j, k) => i,
                ValuesProvider = valuesProvider,
                StreamToApply = resourceStream
            }).Output;
        }
    }
}
