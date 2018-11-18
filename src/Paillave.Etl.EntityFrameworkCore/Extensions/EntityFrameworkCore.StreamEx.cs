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
        public static IStream<TOut> CrossApplyEntityFrameworkCoreQuery<TIn, TResource, TOut>(this IStream<TIn> stream, string name, ISingleStream<TResource> resourceStream, Func<TIn, TResource, IQueryable<TOut>> getQuery, bool noParallelisation = false) where TResource : DbContext
        {
            return stream.CrossApply(name, resourceStream, (TIn inputValue, TResource valueToApply, Action<TOut> push) =>
            {
                foreach (var item in getQuery(inputValue, valueToApply).ToList())
                    push(item);
            }, noParallelisation);
        }
        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, SaveMode bulkLoadMode = SaveMode.BulkInsert, int chunkSize = 1000)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, IStream<TIn>>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, IStream<TIn>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughEntityFrameworkCore<TIn, TResource, TKey>(this ISortedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, SaveMode bulkLoadMode = SaveMode.BulkInsert, int chunkSize = 1000)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, ISortedStream<TIn, TKey>>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, ISortedStream<TIn, TKey>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughEntityFrameworkCore<TIn, TResource, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, SaveMode bulkLoadMode = SaveMode.BulkInsert, int chunkSize = 1000)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, IKeyedStream<TIn, TKey>>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, IKeyedStream<TIn, TKey>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode
            }).Output;
        }
        public static ISingleStream<TIn> ThroughEntityFrameworkCore<TIn, TResource>(this ISingleStream<TIn> stream, string name, IStream<TResource> resourceStream, SaveMode bulkLoadMode = SaveMode.BulkInsert, int chunkSize = 1000)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, ISingleStream<TIn>>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, ISingleStream<TIn>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode
            }).Output;
        }









        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TResource, TEntityKey>(this IStream<TIn> stream, string name, IStream<TResource> resourceStream, Expression<Func<TIn, TEntityKey>> getBusinessKey, int chunkSize = 100)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, IStream<TIn>, TEntityKey>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, IStream<TIn>, TEntityKey>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                GetKey = getBusinessKey
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughEntityFrameworkCore<TIn, TResource, TKey, TEntityKey>(this ISortedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, Expression<Func<TIn, TEntityKey>> getBusinessKey, int chunkSize = 100)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, ISortedStream<TIn, TKey>, TEntityKey>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, ISortedStream<TIn, TKey>, TEntityKey>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                GetKey = getBusinessKey
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughEntityFrameworkCore<TIn, TResource, TKey, TEntityKey>(this IKeyedStream<TIn, TKey> stream, string name, IStream<TResource> resourceStream, Expression<Func<TIn, TEntityKey>> getBusinessKey, int chunkSize = 100)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, IKeyedStream<TIn, TKey>, TEntityKey>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, IKeyedStream<TIn, TKey>, TEntityKey>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                GetKey = getBusinessKey
            }).Output;
        }
        public static ISingleStream<TIn> ThroughEntityFrameworkCore<TIn, TResource, TEntityKey>(this ISingleStream<TIn> stream, string name, IStream<TResource> resourceStream, Expression<Func<TIn, TEntityKey>> getBusinessKey, int chunkSize = 100)
            where TResource : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TResource, ISingleStream<TIn>, TEntityKey>(name, new ThroughEntityFrameworkCoreArgs<TIn, TResource, ISingleStream<TIn>, TEntityKey>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                GetKey = getBusinessKey
            }).Output;
        }
    }
}
