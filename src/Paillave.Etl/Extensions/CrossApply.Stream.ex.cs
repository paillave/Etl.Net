using System;
using System.Collections.Generic;
using System.Threading;

namespace Paillave.Etl.Core
{
    public static partial class CrossApplyEx
    {
        public static IStream<TOut> CrossApply<TIn, TOut>(this IStream<TIn> stream, string name, IValuesProvider<TIn, TOut> valuesProvider, bool noParallelisation = false) =>
            new CrossApplyStreamNode<TIn, TOut>(name, new CrossApplyArgs<TIn, TOut>
            {
                NoParallelisation = noParallelisation,
                Stream = stream,
                ValuesProvider = valuesProvider
            }).Output;
        public static IStream<Correlated<TOut>> CrossApply<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, IValuesProvider<TIn, TOut> valuesProvider, bool noParallelisation = false) =>
            new CrossApplyStreamNode<Correlated<TIn>, Correlated<TOut>>(name, new CrossApplyArgs<Correlated<TIn>, Correlated<TOut>>
            {
                NoParallelisation = noParallelisation,
                Stream = stream,
                ValuesProvider = new ValueProviderCorrelationWrapper<TIn, TOut>(valuesProvider)
            }).Output;
        public static IStream<Correlated<TOut>> CrossApply<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, IEnumerable<TOut>> values, bool noParallelisation = false)
            => stream.CrossApply<Correlated<TIn>, Correlated<TOut>>(name, new ValueProviderCorrelationWrapper<TIn, TOut>(EnumerableValuesProvider.Create(values)), noParallelisation);
        public static IStream<Correlated<TOut>> CrossApply<TIn, TOut>(this IStream<Correlated<TIn>> stream, string name, Action<TIn, IExecutionContext, CancellationToken, Action<TOut>> pushValues, bool noParallelisation = false)
            => stream.CrossApply<Correlated<TIn>, Correlated<TOut>>(name, new ValueProviderCorrelationWrapper<TIn, TOut>(SimpleValuesProvider.Create(pushValues)), noParallelisation);
        public static IStream<TOut> CrossApply<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<TOut>> values, bool noParallelisation = false)
            => stream.CrossApply<TIn, TOut>(name, EnumerableValuesProvider.Create(values), noParallelisation);
        public static IStream<TOut> CrossApply<TIn, TOut>(this IStream<TIn> stream, string name, Action<TIn, IExecutionContext, CancellationToken, Action<TOut>> pushValues, bool noParallelisation = false)
            => stream.CrossApply<TIn, TOut>(name, SimpleValuesProvider.Create(pushValues), noParallelisation);
        public static IStream<TOut> CrossApplyNonCorrelated<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<TOut>> values, bool noParallelisation = false)
            => stream.CrossApply<TIn, TOut>(name, EnumerableValuesProvider.Create(values), noParallelisation);
        public static IStream<TOut> CrossApplyNonCorrelated<TIn, TOut>(this IStream<TIn> stream, string name, Action<TIn, IExecutionContext, CancellationToken, Action<TOut>> pushValues, bool noParallelisation = false)
            => stream.CrossApply<TIn, TOut>(name, SimpleValuesProvider.Create(pushValues), noParallelisation);
    }
}
