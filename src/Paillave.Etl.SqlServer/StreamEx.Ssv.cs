using Paillave.Etl.Core.Streams;
using Paillave.Etl.SqlServer.StreamNodes;
using Paillave.Etl.SqlServer.ValuesProviders;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace Paillave.Etl
{
    public static class StreamExSsv
    {
        public static IStream<TOut> CrossApplySqlServerQuery<TIn, TRes, TOut>(this IStream<TIn> stream, string name, IStream<SqlConnection> sqlConnection, string sqlQuery, bool noParallelisation = false)
        {
            return stream.CrossApply(name, sqlConnection, new SqlCommandValueProvider<TIn, TOut>(new SqlCommandValueProviderArgs<TIn, TOut>()
            {
                SqlQuery = sqlQuery,
                NoParallelisation = noParallelisation
            }));
        }
        public static IStream<TIn> ToSqlServer<TIn, TRes>(this IStream<TIn> stream, string name, IStream<SqlConnection> sqlConnection)
            where TIn : class
        {
            return new ToSqlCommandStreamNode<TIn, IStream<TIn>>(name, new ToSqlCommandArgs<TIn, IStream<TIn>>
            {
                SourceStream = stream
            }).Output;
        }
    }
}
