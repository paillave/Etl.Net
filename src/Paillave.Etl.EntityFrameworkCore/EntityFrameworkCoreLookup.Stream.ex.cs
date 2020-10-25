using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.Core;

namespace Paillave.Etl.EntityFrameworkCore
{
    public static class EntityFrameworkCoreLookupEx
    {
        public static IStream<TOut> EfCoreLookup<TIn, TEntity, TOut, TKey>(
            this IStream<TIn> inputStream,
            string name,
            Func<EfCoreLookupArgsBuilder<TIn>, EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey>> getOptions)
                where TEntity : class
                    => new EfCoreLookupStreamNode<TIn, TIn, TEntity, TOut, TKey, TOut>(name, getOptions(new EfCoreLookupArgsBuilder<TIn>(inputStream)).BuildArgs()).Output;

        public static IStream<Correlated<TOut>> EfCoreLookup<TIn, TEntity, TOut, TKey>(
            this IStream<Correlated<TIn>> inputStream,
            string name,
            Func<EfCoreLookupCorrelatedArgsBuilder<TIn>, EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TOut, TKey>> getOptions)
                where TEntity : class
                    => new EfCoreLookupStreamNode<Correlated<TIn>, TIn, TEntity, TOut, TKey, Correlated<TOut>>(name, getOptions(new EfCoreLookupCorrelatedArgsBuilder<TIn>(inputStream)).BuildArgs()).Output;
    }
}