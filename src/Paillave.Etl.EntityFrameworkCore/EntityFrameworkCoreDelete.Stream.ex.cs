using System;
using Paillave.Etl.Core;

namespace Paillave.Etl.EntityFrameworkCore
{
    public static class EntityFrameworkCoreDeleteEx
    {
        public static IStream<TIn> EfCoreDelete<TIn, TEntity>(this IStream<TIn> inputStream, string name, Func<DeleteEntityFrameworkCoreArgsBuilder<TIn>, DeleteEntityFrameworkCoreArgsBuilder<TIn, TEntity>> builder)
            where TEntity : class
        {
            var ar = builder(new DeleteEntityFrameworkCoreArgsBuilder<TIn>(inputStream)).BuildArgs();
            return new DeleteEntityFrameworkCoreStreamNode<TIn, TIn, TEntity>(name, ar).Output;
        }
        public static IStream<Correlated<TIn>> EfCoreDelete<TIn, TEntity>(this IStream<Correlated<TIn>> inputStream, string name, Func<DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn>, DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn, TEntity>> builder)
            where TEntity : class
        {
            var ar = builder(new DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn>(inputStream)).BuildArgs();
            return new DeleteEntityFrameworkCoreStreamNode<Correlated<TIn>, TIn, TEntity>(name, ar).Output;
        }
    }
}