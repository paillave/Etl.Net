using Paillave.Etl;
using Paillave.Etl.Extensions;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.SqlServer.StreamNodes;
using Paillave.Etl.SqlServer.ValuesProviders;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace Paillave.Etl.SqlServer.Extensions
{
    public static class SqlServerEx
    {
        public static IStream<TOut> CrossApplySqlServerQuery<TIn, TOut>(this IStream<TIn> stream, string name, Func<SqlCommandValueProviderArgsBuilder<TIn>, SqlCommandValueProviderArgsBuilder<TIn, TOut>> buildArgs, bool noParallelisation = false)
        {
            var valuesProvider = new SqlCommandValueProvider<TIn, TOut>(buildArgs(new SqlCommandValueProviderArgsBuilder<TIn>()).GetArgs());
            return stream.CrossApply(name, valuesProvider, noParallelisation);
        }
        public static IStream<TIn> ThroughSqlServer<TIn>(this IStream<TIn> stream, string name, string sqlQuery, string connectionName = null)
            where TIn : class
        {
            return new ThroughSqlCommandStreamNode<TIn, IStream<TIn>>(name, new ThroughSqlCommandArgs<TIn, IStream<TIn>>
            {
                SourceStream = stream,
                ConnectionName = connectionName,
                SqlQuery = sqlQuery
            }).Output;
        }
    }
}
