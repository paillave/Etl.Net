using Paillave.Etl.Core;
using System;
using System.Linq;

namespace Paillave.Etl.EntityFrameworkCore
{
    public static class EntityFrameworkCoreReadEx
    {
        public static IStream<TOut> EfCoreSelect<TIn, TOut>(this IStream<TIn> stream, string name,
            Func<DbContextWrapper, TIn, IQueryable<TOut>> getQuery, bool streamMode = false, string connectionKey = null, bool noParallelisation = false) where TOut : class
                => stream.CrossApply<TIn, TOut>(name, new EfCoreValuesProvider<TIn, TOut>(
                    new EfCoreValuesProviderArgs<TIn, TOut>
                    {
                        GetQuery = getQuery,
                        ConnectionKey = connectionKey,
                        StreamMode = streamMode
                    }), noParallelisation);
        public static ISingleStream<TOut> EfCoreSelectSingle<TIn, TOut>(this ISingleStream<TIn> stream, string name,
            Func<DbContextWrapper, TIn, IQueryable<TOut>> getQuery, string connectionKey = null) where TOut : class
                => new EfCoreSelectSingleStreamNode<TIn, TOut>(name, new EfCoreSingleValueProviderArgs<TIn, TOut>
                {
                    Stream = stream,
                    GetQuery = getQuery,
                    ConnectionKey = connectionKey
                }).Output;
        public static IStream<TOut> EfCoreSelectSingle<TIn, TOut>(this IStream<TIn> stream, string name,
            Func<DbContextWrapper, TIn, IQueryable<TOut>> getQuery, string connectionKey = null, bool noParallelisation = false) where TOut : class
                => stream.CrossApply<TIn, TOut>(name, new EfCoreSingleValueProvider<TIn, TOut>(
                    new EfCoreSingleValueProviderArgs<TIn, TOut>
                    {
                        Stream = stream,
                        GetQuery = getQuery,
                        ConnectionKey = connectionKey
                    }), noParallelisation);
        //TODO: make a version for correlated
    }
}