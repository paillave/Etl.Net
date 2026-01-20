using Paillave.Etl.Core;
using System;
using System.Linq.Expressions;

namespace Paillave.Etl.EntityFrameworkCore;

public static class EntityFrameworkCoreUpdateEx
{
    public static IStream<TIn> EfCoreUpdate<TIn, TEntity>(this IStream<TIn> stream, string name, Expression<Func<TIn, TEntity>> updateKey, Expression<Func<TIn, TEntity>> updateValues, UpdateMode updateMode = UpdateMode.SqlServerBulk, int chunkSize = 10000, string? connectionKey = null)
        where TEntity : class
    {
        return new UpdateEntityFrameworkCoreStreamNode<TEntity, TIn>(name, new UpdateEntityFrameworkCoreArgs<TEntity, TIn>
        {
            SourceStream = stream,
            BatchSize = chunkSize,
            BulkLoadMode = updateMode,
            UpdateKey = updateKey,
            UpdateValues = updateValues,
            ConnectionKey = connectionKey
        }).Output;
    }
}