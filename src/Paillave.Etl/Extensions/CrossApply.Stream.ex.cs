using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;

namespace Paillave.Etl.Extensions
{
    public static partial class CrossApplyEx
    {
        public static IStream<TOut> CrossApply<TIn, TOut>(this IStream<TIn> stream, string name, IValuesProvider<TIn, TOut> valuesProvider, bool noParallelisation = false)
        {
            return new CrossApplyStreamNode<TIn, TOut>(name, new CrossApplyArgs<TIn, TOut>
            {
                NoParallelisation = noParallelisation,
                Stream = stream,
                ValuesProvider = valuesProvider
            }).Output;
        }
        public static IStream<Correlated<TOut>> CrossApply<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, IValuesProvider<TIn, TOut> valuesProvider, bool noParallelisation = false)
        {
            return new CrossApplyStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new CrossApplyArgs<Correlated<TIn>, Correlated<TOut>>
            {
                NoParallelisation = noParallelisation,
                Stream = stream,
                ValuesProvider = new ValuesProviders.ValueProviderCorrelationWrapper<TIn, TOut>(valuesProvider)
            }).Output;
        }
    }
}
