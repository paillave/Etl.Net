using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.EntityFrameworkCore.StreamNodes;
using System;
using System.Linq;
using Paillave.Etl;
using Paillave.Etl.Extensions;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace Paillave.Etl.EntityFrameworkCore.Extensions
{
    public static class EntityFrameworkCoreEx
    {
        public static IStream<TOut> CrossApplyEntityFrameworkCoreQuery<TIn, TResource, TOut>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Func<TIn, TResource, IQueryable<TOut>> getQuery, bool noParallelisation = false) where TResource : DbContext
        {
            return stream.CrossApply(name, dbContextStream, (TIn inputValue, TResource valueToApply, Action<TOut> push) =>
            {
                List<TOut> lsts = null;
                stream.ExecutionContext.InvokeInDedicatedThread(valueToApply, () => { lsts = getQuery(inputValue, valueToApply).ToList(); });
                foreach (var item in lsts)
                    push(item);
            }, noParallelisation);
        }

        public static IStream<TOut> CrossApplyEntityFrameworkCoreQuery<TResource, TOut>(this ISingleStream<TResource> stream, string name, Func<TResource, IQueryable<TOut>> getQuery) where TResource : DbContext
        {
            return stream.CrossApply(name, (TResource ctx, Action<TOut> push) =>
            {
                List<TOut> lsts = null;
                stream.ExecutionContext.InvokeInDedicatedThread(ctx, () => { lsts = getQuery(ctx).ToList(); });
                foreach (var item in lsts)
                    push(item);
            }, true);
        }

        public static IStream<TIn> UpdateEntityFrameworkCore<TIn, TResource, TEntity>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Expression<Func<TIn, TEntity>> updateKey, Expression<Func<TIn, TEntity>> updateValues, UpdateMode updateMode = UpdateMode.SqlServerBulk, int chunkSize = 10000)
            where TResource : DbContext
            where TEntity : class
        {
            return new UpdateEntityFrameworkCoreStreamNode<TEntity, TResource, TIn>(name, new UpdateEntityFrameworkCoreArgs<TEntity, TResource, TIn>
            {
                SourceStream = stream,
                DbContextStream = dbContextStream,
                BatchSize = chunkSize,
                BulkLoadMode = updateMode,
                UpdateKey = updateKey,
                UpdateValues = updateValues
            }).Output;
        }

        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, SaveMode bulkLoadMode = SaveMode.SqlServerBulk, int chunkSize = 10000)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, TIn, TIn>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, TIn, TIn>
            {
                SourceStream = stream,
                DbContextStream = dbContextStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode,
                GetEntity = (i, c) => i,
                GetOutput = (i, j) => j,
            }).Output;
        }

        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Expression<Func<TIn, object>> pivotKey, SaveMode saveMode = SaveMode.SqlServerBulk, int chunkSize = 10000)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, TIn, TIn>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, TIn, TIn>
            {
                SourceStream = stream,
                DbContextStream = dbContextStream,
                BatchSize = chunkSize,
                PivotKey = pivotKey,
                BulkLoadMode = saveMode,
                GetEntity = (i, c) => i,
                GetOutput = (i, j) => i,
            }).Output;
        }

        public static IStream<TOut> ThroughEntityFrameworkCore<TIn, TResource, TOut, TInEf>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Func<TIn, TResource, TInEf> getEntity, Func<TIn, TInEf, TOut> getResult, SaveMode bulkLoadMode = SaveMode.SqlServerBulk, int chunkSize = 10000)
            where TResource : DbContext
            where TInEf : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TInEf, TResource, TIn, TOut>(name, new ThroughEntityFrameworkCoreArgs<TInEf, TResource, TIn, TOut>
            {
                SourceStream = stream,
                DbContextStream = dbContextStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode,
                GetEntity = getEntity,
                GetOutput = getResult,
            }).Output;
        }

        public static IStream<TOut> ThroughEntityFrameworkCore<TIn, TResource, TOut, TInEf>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Func<TIn, TResource, TInEf> getEntity, Expression<Func<TInEf, object>> pivotKey, Func<TIn, TInEf, TOut> getResult, SaveMode bulkInsertMode = SaveMode.SqlServerBulk, int chunkSize = 10000)
            where TResource : DbContext
            where TInEf : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TInEf, TResource, TIn, TOut>(name, new ThroughEntityFrameworkCoreArgs<TInEf, TResource, TIn, TOut>
            {
                SourceStream = stream,
                DbContextStream = dbContextStream,
                BatchSize = chunkSize,
                PivotKey = pivotKey,
                BulkLoadMode = bulkInsertMode,
                GetEntity = getEntity,
                GetOutput = getResult,
            }).Output;
        }

        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource, TInEf>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Func<TIn, TResource, TInEf> getEntity, Expression<Func<TInEf, object>> pivotKey, SaveMode bulkInsertMode = SaveMode.SqlServerBulk, int chunkSize = 10000)
            where TResource : DbContext
            where TInEf : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TInEf, TResource, TIn, TIn>(name, new ThroughEntityFrameworkCoreArgs<TInEf, TResource, TIn, TIn>
            {
                SourceStream = stream,
                DbContextStream = dbContextStream,
                BatchSize = chunkSize,
                PivotKey = pivotKey,
                BulkLoadMode = bulkInsertMode,
                GetEntity = getEntity,
                GetOutput = (i, j) => i,
            }).Output;
        }

        public static IStream<TOut> EntityFrameworkCoreLookup<TIn, TEntity, TCtx, TOut, TKey>(this IStream<TIn> inputStream, string name, ISingleStream<TCtx> dbContextStream, Expression<Func<TIn, TKey>> getLeftStreamKey, Expression<Func<TEntity, TKey>> getEntityStreamKey, Func<TIn, TEntity, TOut> resultSelector, Expression<Func<TEntity, bool>> defaultCriteria, bool getFullDataset = false, Func<IQueryable<TEntity>, IQueryable<TEntity>> includeInstruction = null, Func<TIn, TCtx, TEntity> createIfNotFound = null, int cacheSize = 10000)
            where TCtx : DbContext
            where TEntity : class
        {
            return new LookupEntityFrameworkCoreStreamNode<TIn, TEntity, TCtx, TOut, TKey>(name, new LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut, TKey>
            {
                CacheSize = cacheSize,
                DbContextStream = dbContextStream,
                InputStream = inputStream,
                GetEntityStreamKey = getEntityStreamKey,
                GetLeftStreamKey = getLeftStreamKey,
                ResultSelector = resultSelector,
                CreateIfNotFound = createIfNotFound,
                DefaultCriteria = defaultCriteria,
                GetFullDataset = getFullDataset,
                IncludeInstruction = includeInstruction
            }).Output;
        }

        public static IStream<TOut> EntityFrameworkCoreLookup<TIn, TEntity, TCtx, TOut, TKey>(this IStream<TIn> inputStream, string name, ISingleStream<TCtx> dbContextStream, Expression<Func<TIn, TKey>> getLeftStreamKey, Expression<Func<TEntity, TKey>> getEntityStreamKey, Func<TIn, TEntity, TOut> resultSelector, Func<TIn, TCtx, TEntity> createIfNotFound = null, int cacheSize = 10000)
            where TCtx : DbContext
            where TEntity : class
        {
            return new LookupEntityFrameworkCoreStreamNode<TIn, TEntity, TCtx, TOut, TKey>(name, new LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut, TKey>
            {
                CacheSize = cacheSize,
                DbContextStream = dbContextStream,
                InputStream = inputStream,
                GetEntityStreamKey = getEntityStreamKey,
                GetLeftStreamKey = getLeftStreamKey,
                ResultSelector = resultSelector,
                CreateIfNotFound = createIfNotFound
            }).Output;
        }

        public static IStream<TIn> EntityFrameworkCoreDelete<TIn, TEntity, TCtx>(this IStream<TIn> inputStream, string name, ISingleStream<TCtx> dbContextStream, Expression<Func<TIn, TEntity, bool>> match)
            where TCtx : DbContext
            where TEntity : class
        {
            return new DeleteEntityFrameworkCoreStreamNode<TIn, TEntity, TCtx>(name, new DeleteEntityFrameworkCoreArgs<TIn, TEntity, TCtx>
            {
                DbContextStream = dbContextStream,
                InputStream = inputStream,
                Match = match,
            }).Output;
        }
    }
}