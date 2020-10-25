using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.Extensions;

namespace Paillave.Etl.EntityFrameworkCore
{
    public static class EntityFrameworkCoreReadEx
    {
        public static IStream<TOut> EfCoreSelect<TIn, TOut>(this IStream<TIn> stream, string name,
            Func<EfCoreValuesProviderArgsBuilder<TIn>, EfCoreValuesProviderArgsBase<TIn, TOut>> createParameters, bool noParallelisation = false) where TOut : class
                => stream.CrossApply<TIn, TOut>(name, new EfCoreValuesProvider<TIn, TOut>(new EfCoreValuesProviderArgs<TIn, TOut>
                {
                    InputStream = stream,
                    Arguments = createParameters(new EfCoreValuesProviderArgsBuilder<TIn>())
                }), noParallelisation);
    }
}