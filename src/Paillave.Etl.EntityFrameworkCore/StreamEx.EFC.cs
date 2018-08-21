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
        public static IStream<TOut> CrossApplyEntityFrameworkQuery<TIn, TRes, TOut>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream, Func<TIn, TRes, IQueryable<TOut>> getQuery, bool noParallelisation = false) where TRes : DbContext
        {
            return stream.CrossApply(name, resourceStream, new EntityFrameworkCoreValueProvider<TIn, TRes, TOut>(new EntityFrameworkCoreValueProviderArgs<TIn, TRes, TOut>()
            {
                GetQuery = getQuery,
                NoParallelisation = noParallelisation
            }));
        }
        public static IStream<TIn> ToEntityFrameworkCore<TIn, TRes>(this IStream<TIn> stream, string name, IStream<TRes> resourceStream)
            where TRes : DbContext
            where TIn : class
        {
            return new ToEntityFrameworkStreamNode<TIn, TRes>(stream, name, new Core.StreamNodes.ToResourceStreamArgsBase<TRes>
            {
                ResourceStream = resourceStream
            }).Output;
        }
    }
}
