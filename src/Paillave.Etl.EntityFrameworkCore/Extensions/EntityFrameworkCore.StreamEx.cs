using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.EntityFrameworkCore.StreamNodes;
using System;
using System.Linq;
using Paillave.Etl;
using Paillave.Etl.Extensions;
using System.Linq.Expressions;

namespace Paillave.Etl.EntityFrameworkCore.Extensions
{
    public static class EntityFrameworkCoreEx
    {
        public static IStream<TOut> CrossApplyEntityFrameworkCoreQuery<TIn, TResource, TOut>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Func<TIn, TResource, IQueryable<TOut>> getQuery, bool noParallelisation = false) where TResource : DbContext
        {
            return stream.CrossApply(name, dbContextStream, (TIn inputValue, TResource valueToApply, Action<TOut> push) =>
            {
                foreach (var item in getQuery(inputValue, valueToApply).ToList())
                    push(item);
            }, noParallelisation);
        }
        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, SaveMode bulkLoadMode = SaveMode.Bulk, int chunkSize = 10000)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, TIn, TIn>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, TIn, TIn>
            {
                SourceStream = stream,
                DbContextStream = dbContextStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode,
                GetEntity = (i,c) => i,
                GetOutput = (i, j) => j,
            }).Output;
        }

        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Expression<Func<TIn, object>> pivotKey, SaveMode saveMode = SaveMode.Bulk, int chunkSize = 10000)
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
                GetEntity = (i,c) => i,
                GetOutput = (i, j) => i,
            }).Output;
        }

        //public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Expression<Func<TIn, TIn, bool>> compare, int chunkSize = 10000)
        //    where TResource : DbContext
        //    where TIn : class
        //{
        //    return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, TIn, TIn>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, TIn, TIn>
        //    {
        //        SourceStream = stream,
        //        DbContextStream = dbContextStream,
        //        BatchSize = chunkSize,
        //        Compare = compare,
        //        BulkLoadMode = SaveMode.StandardEfCoreUpsert,
        //        GetEntity = i => i,
        //        GetOutput = (i, j) => i,
        //    }).Output;
        //}

        public static IStream<TOut> ThroughEntityFrameworkCore<TIn, TResource, TOut, TInEf>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Func<TIn,TResource ,TInEf> getEntity, Func<TIn, TInEf, TOut> getResult, SaveMode bulkLoadMode = SaveMode.Bulk, int chunkSize = 10000)
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

        public static IStream<TOut> ThroughEntityFrameworkCore<TIn, TResource, TOut, TInEf>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Func<TIn, TResource, TInEf> getEntity, Expression<Func<TInEf, object>> pivotKey, Func<TIn, TInEf, TOut> getResult, SaveMode bulkInsertMode = SaveMode.Bulk, int chunkSize = 10000)
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

        //public static IStream<TOut> ThroughEntityFrameworkCore<TIn, TResource, TOut, TInEf>(this IStream<TIn> stream, string name, ISingleStream<TResource> dbContextStream, Func<TIn, TInEf> getEntity, Expression<Func<TInEf, TInEf, bool>> compare, Func<TIn, TInEf, TOut> getResult, int chunkSize = 10000)
        //    where TResource : DbContext
        //    where TInEf : class
        //{
        //    return new ThroughEntityFrameworkCoreStreamNode<TInEf, TResource, TIn, TOut>(name, new ThroughEntityFrameworkCoreArgs<TInEf, TResource, TIn, TOut>
        //    {
        //        SourceStream = stream,
        //        DbContextStream = dbContextStream,
        //        BatchSize = chunkSize,
        //        Compare = compare,
        //        BulkLoadMode = SaveMode.StandardEfCoreUpsert,
        //        GetEntity = getEntity,
        //        GetOutput = getResult,
        //    }).Output;
        //}
        public static IStream<TOut> EntityFrameworkCoreLookup<TIn, TEntity, TCtx, TOut>(this IStream<TIn> inputStream, string name, ISingleStream<TCtx> dbContextStream, Expression<Func<TIn, TEntity, bool>> match, Func<TIn, TEntity, TOut> resultSelector, int cacheSize = 10000)
            where TCtx : DbContext
            where TEntity : class
        {
            return new LookupEntityFrameworkCoreStreamNode<TIn, TEntity, TCtx, TOut>(name, new LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut>
            {
                CacheSize = cacheSize,
                DbContextStream = dbContextStream,
                InputStream = inputStream,
                Match = match,
                ResultSelector = resultSelector
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
