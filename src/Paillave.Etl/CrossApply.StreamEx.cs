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
    public static partial class CrossApplyEx
    {
        public static IStream<TOut> CrossApply<TIn, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, IValuesProvider<TValueIn, TValueOut> valuesProvider, Func<TIn, TValueIn> inputValueSelector, Func<TValueOut, TIn, TOut> outputValueSelector)
        {
            return new CrossApplyStreamNode<TIn, TValueIn, TValueOut, TOut>(name, new CrossApplyArgs<TIn, TValueIn, TValueOut, TOut>
            {
                Stream = stream,
                GetValueIn = inputValueSelector,
                GetValueOut = outputValueSelector,
                ValuesProvider = valuesProvider
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TOut>(this IStream<TIn> stream, string name, IValuesProvider<TIn, TOut> valuesProvider)
        {
            return new CrossApplyStreamNode<TIn, TIn, TOut, TOut>(name, new CrossApplyArgs<TIn, TIn, TOut, TOut>
            {
                Stream = stream,
                GetValueIn = i => i,
                GetValueOut = (i, j) => i,
                ValuesProvider = valuesProvider
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TRes, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, IValuesProvider<TValueIn, TRes, TValueOut> valuesProvider, Func<TIn, TRes, TValueIn> inputValueSelector, Func<TValueOut, TIn, TRes, TOut> outputValueSelector)
        {
            return new CrossApplyStreamNode<TIn, TRes, TValueIn, TValueOut, TOut>(name, new CrossApplyArgs<TIn, TRes, TValueIn, TValueOut, TOut>
            {
                MainStream = stream,
                GetValueIn = inputValueSelector,
                GetValueOut = outputValueSelector,
                ValuesProvider = valuesProvider,
                StreamToApply = resourceStream
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TRes, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, IValuesProvider<TIn, TRes, TOut> valuesProvider)
        {
            return new CrossApplyStreamNode<TIn, TRes, TIn, TOut, TOut>(name, new CrossApplyArgs<TIn, TRes, TIn, TOut, TOut>
            {
                MainStream = stream,
                GetValueIn = (i, j) => i,
                GetValueOut = (i, j, k) => i,
                ValuesProvider = valuesProvider,
                StreamToApply = resourceStream
            }).Output;
        }
    }
}
