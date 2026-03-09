using Paillave.Etl.Core;
using System;
using System.Linq.Expressions;

namespace Paillave.Etl.SqlServer;

public static class SqlServerEx
{
    public static IStream<TOut> CrossApplySqlServerQuery<TIn, TOut>(this IStream<TIn> stream, string name, Func<SqlCommandValueProviderArgsBuilder<TIn>, SqlCommandValueProviderArgsBuilder<TIn, TOut>> buildArgs, string connectionName = null, bool noParallelisation = false)
    {
        var valuesProvider = new SqlCommandValueProvider<TIn, TOut>(buildArgs(new SqlCommandValueProviderArgsBuilder<TIn>(connectionName)).GetArgs());
        return stream.CrossApply(name, valuesProvider, noParallelisation);
    }
    public static IStream<TIn> ToSqlCommand<TIn>(this IStream<TIn> stream, string name, string sqlQuery, string connectionName = null)
        where TIn : class
    {
        return new ToSqlCommandStreamNode<TIn, IStream<TIn>, TIn>(name, new ToSqlCommandArgs<TIn, IStream<TIn>, TIn>
        {
            SourceStream = stream,
            ConnectionName = connectionName,
            SqlQuery = sqlQuery,
            GetValue = i => i
        }).Output;
    }
    public static IStream<Correlated<TIn>> ToSqlCommand<TIn>(this IStream<Correlated<TIn>> stream, string name, string sqlQuery, string connectionName = null)
        where TIn : class
    {
        return new ToSqlCommandStreamNode<Correlated<TIn>, IStream<Correlated<TIn>>, TIn>(name, new ToSqlCommandArgs<Correlated<TIn>, IStream<Correlated<TIn>>, TIn>
        {
            SourceStream = stream,
            ConnectionName = connectionName,
            SqlQuery = sqlQuery,
            GetValue = i => i.Row
        }).Output;
    }
    public static IStream<TIn> SqlServerSave<TIn>(this IStream<TIn> stream, string name, Func<SqlServerSaveCommandArgsBuilder<TIn, TIn>, SqlServerSaveCommandArgsBuilder<TIn, TIn>> buildArgs = null)
        where TIn : class
    {
        if (buildArgs == null) buildArgs = i => i;
        return new SqlServerSaveStreamNode<TIn, IStream<TIn>, TIn>(name, buildArgs(new SqlServerSaveCommandArgsBuilder<TIn, TIn>(i => i)).GetArgs(stream)).Output;
    }
    public static IStream<Correlated<TIn>> SqlServerSave<TIn>(this IStream<Correlated<TIn>> stream, string name, Func<SqlServerSaveCommandArgsBuilder<Correlated<TIn>, TIn>, SqlServerSaveCommandArgsBuilder<Correlated<TIn>, TIn>> buildArgs = null)
        where TIn : class
    {
        if (buildArgs == null) buildArgs = i => i;
        return new SqlServerSaveStreamNode<Correlated<TIn>, IStream<Correlated<TIn>>, TIn>(name, buildArgs(new SqlServerSaveCommandArgsBuilder<Correlated<TIn>, TIn>(i => i.Row)).GetArgs(stream)).Output;
    }
}
