using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.Extensions;
using System.Linq;

namespace Paillave.Etl.EntityFrameworkCore
{
    public static class EntityFrameworkCoreReadEx
    {
        public static IStream<TOut> EfCoreSelect<TIn, TOut>(this IStream<TIn> stream, string name,
            Func<DbContextWrapper, TIn, IQueryable<TOut>> getQuery, string connectionKey = null, bool noParallelisation = false) where TOut : class
                => stream.CrossApply<TIn, TOut>(name, new EfCoreValuesProvider<TIn, TOut>(
                    new EfCoreValuesProviderArgs<TIn, TOut>
                    {
                        GetQuery = getQuery,
                        ConnectionKey = connectionKey
                    }), noParallelisation);
        //TODO: make a version for correlated
    }
}