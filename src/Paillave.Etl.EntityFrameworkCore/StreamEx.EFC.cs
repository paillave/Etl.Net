using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.EntityFrameworkCore.StreamNodes;
using Paillave.Etl.EntityFrameworkCore.ValuesProviders;
using System;
using System.Linq;

namespace Paillave.Etl
{
    public static class StreamExEfc
    {
        public static IStream<TOut> CrossApplyEntityFrameworkCoreQuery<TIn, TRes, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, Func<TIn, TRes, IQueryable<TOut>> getQuery, bool noParallelisation = false) where TRes : DbContext
        {
            return stream.CrossApply(name, resourceStream, new EntityFrameworkCoreValueProvider<TIn, TRes, TOut>(new EntityFrameworkCoreValueProviderArgs<TIn, TRes, TOut>()
            {
                GetQuery = getQuery,
                NoParallelisation = noParallelisation
            }));
        }
        public static IStream<TIn> ThroughEntityFrameworkCore<TIn, TRes>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, BulkLoadMode bulkLoadMode = BulkLoadMode.InsertOnly, int chunkSize = 1000)
            where TRes : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TRes, IStream<TIn>>(name, new ThroughEntityFrameworkCoreArgs<TIn, TRes, IStream<TIn>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ThroughEntityFrameworkCore<TIn, TRes, TKey>(this ISortedStream<TIn, TKey> stream, string name, IStream<TRes> resourceStream, BulkLoadMode bulkLoadMode = BulkLoadMode.InsertOnly, int chunkSize = 1000)
            where TRes : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TRes, ISortedStream<TIn, TKey>>(name, new ThroughEntityFrameworkCoreArgs<TIn, TRes, ISortedStream<TIn, TKey>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ThroughEntityFrameworkCore<TIn, TRes, TKey>(this IKeyedStream<TIn, TKey> stream, string name, IStream<TRes> resourceStream, BulkLoadMode bulkLoadMode = BulkLoadMode.InsertOnly, int chunkSize = 1000)
            where TRes : DbContext
            where TIn : class
        {
            return new ThroughEntityFrameworkCoreStreamNode<TIn, TRes, IKeyedStream<TIn, TKey>>(name, new ThroughEntityFrameworkCoreArgs<TIn, TRes, IKeyedStream<TIn, TKey>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize,
                BulkLoadMode = bulkLoadMode
            }).Output;
        }
    }
}
