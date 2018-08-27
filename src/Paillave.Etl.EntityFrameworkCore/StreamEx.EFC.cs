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
        public static IStream<TIn> ToEntityFrameworkCore<TIn, TRes>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, int chunkSize = 1000)
            where TRes : DbContext
            where TIn : class
        {
            return new ToEntityFrameworkCoreStreamNode<TIn, TRes, IStream<TIn>>(name, new ToEntityFrameworkCoreArgs<TIn, TRes, IStream<TIn>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize
            }).Output;
        }
        public static ISortedStream<TIn> ToEntityFrameworkCore<TIn, TRes>(this ISortedStream<TIn> stream, string name, IStream<TRes> resourceStream, int chunkSize = 1000)
            where TRes : DbContext
            where TIn : class
        {
            return new ToEntityFrameworkCoreStreamNode<TIn, TRes, ISortedStream<TIn>>(name, new ToEntityFrameworkCoreArgs<TIn, TRes, ISortedStream<TIn>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize
            }).Output;
        }
        public static IKeyedStream<TIn> ToEntityFrameworkCore<TIn, TRes>(this IKeyedStream<TIn> stream, string name, IStream<TRes> resourceStream, int chunkSize = 1000)
            where TRes : DbContext
            where TIn : class
        {
            return new ToEntityFrameworkCoreStreamNode<TIn, TRes, IKeyedStream<TIn>>(name, new ToEntityFrameworkCoreArgs<TIn, TRes, IKeyedStream<TIn>>
            {
                SourceStream = stream,
                DbContextStream = resourceStream,
                BatchSize = chunkSize
            }).Output;
        }
    }
}
