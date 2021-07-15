using Paillave.Etl.Core;
using System;
using System.Linq.Expressions;

namespace Paillave.Etl.SqlServer
{
    public static class SqlServerEx
    {
        public static IStream<TOut> CrossApplySqlServerQuery<TIn, TOut>(this IStream<TIn> stream, string name, Func<SqlCommandValueProviderArgsBuilder<TIn>, SqlCommandValueProviderArgsBuilder<TIn, TOut>> buildArgs, bool noParallelisation = false)
        {
            var valuesProvider = new SqlCommandValueProvider<TIn, TOut>(buildArgs(new SqlCommandValueProviderArgsBuilder<TIn>()).GetArgs());
            return stream.CrossApply(name, valuesProvider, noParallelisation);
        }
        public static IStream<TIn> ToSqlCommand<TIn>(this IStream<TIn> stream, string name, string sqlQuery, string connectionName = null)
            where TIn : class
        {
            return new ToSqlCommandStreamNode<TIn, IStream<TIn>>(name, new ToSqlCommandArgs<TIn, IStream<TIn>>
            {
                SourceStream = stream,
                ConnectionName = connectionName,
                SqlQuery = sqlQuery
            }).Output;
        }
        public static IStream<TIn> SqlServerSave<TIn>(this IStream<TIn> stream, string name, string table, Expression<Func<TIn, object>> pivot = null, Expression<Func<TIn, object>> computed = null, string connectionName = null)
            where TIn : class
        {
            return new SqlServerSaveStreamNode<TIn, IStream<TIn>>(name, new SqlServerSaveCommandArgs<TIn, IStream<TIn>>
            {
                Table = table,
                SourceStream = stream,
                ConnectionName = connectionName,
                Pivot = pivot,
                Computed = computed
            }).Output;
        }
    }
}
