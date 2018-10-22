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
        public static IStream<TOut> CrossApply<TIn, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, IValuesProvider<TValueIn, TValueOut> valuesProvider, Func<TIn, TValueIn> preProcessValue, Func<TValueOut, TIn, TOut> postProcessValue)
        {
            return new CrossApplyStreamNode<TIn, TValueIn, TValueOut, TOut>(name, new CrossApplyArgs<TIn, TValueIn, TValueOut, TOut>
            {
                Stream = stream,
                GetValueIn = preProcessValue,
                GetValueOut = postProcessValue,
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
        public static IStream<TOut> CrossApply<TIn, TResource, TValueIn, TValueOut, TOut>(this IStream<TIn> stream, string name, ISingleStream<TResource> resourceStream, IValuesProvider<TValueIn, TResource, TValueOut> valuesProvider, Func<TIn, TResource, TValueIn> preProcessValue, Func<TValueOut, TIn, TResource, TOut> postProcessValue)
        {
            return new CrossApplyStreamNode<TIn, TResource, TValueIn, TValueOut, TOut>(name, new CrossApplyArgs<TIn, TResource, TValueIn, TValueOut, TOut>
            {
                MainStream = stream,
                GetValueIn = preProcessValue,
                GetValueOut = postProcessValue,
                ValuesProvider = valuesProvider,
                StreamToApply = resourceStream
            }).Output;
        }
        public static IStream<TOut> CrossApply<TIn, TResource, TOut>(this IStream<TIn> stream, string name, ISingleStream<TResource> resourceStream, IValuesProvider<TIn, TResource, TOut> valuesProvider)
        {
            return new CrossApplyStreamNode<TIn, TResource, TIn, TOut, TOut>(name, new CrossApplyArgs<TIn, TResource, TIn, TOut, TOut>
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
