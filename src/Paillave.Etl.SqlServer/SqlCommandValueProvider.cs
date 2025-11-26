using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.SqlServer.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Paillave.Etl.SqlServer;

public class SqlCommandValueProviderArgsBuilder<TIn>
{
    private readonly string _connectionName;

    public SqlCommandValueProviderArgsBuilder(string connectionName) => (_connectionName) = (connectionName);
    public SqlCommandValueProviderArgsBuilder() { }

    public SqlCommandValueProviderWithQueryArgsBuilder<TIn> FromQuery(string query)
        => new SqlCommandValueProviderWithQueryArgsBuilder<TIn>(query, _connectionName);
    public SqlCommandValueProviderArgsBuilder<TIn> WithKeyedConnection(string keyedConnection)
        => new SqlCommandValueProviderArgsBuilder<TIn>(keyedConnection);
}
public class SqlCommandValueProviderWithQueryArgsBuilder<TIn>
{
    private readonly string _query;
    private readonly string _connectionName;
    public SqlCommandValueProviderWithQueryArgsBuilder(string query, string connectionName) => (_query, _connectionName) = (query, connectionName);
    public SqlCommandValueProviderArgsBuilder<TIn, TOut> WithMapping<TOut>()
        => new SqlCommandValueProviderArgsBuilder<TIn, TOut>(SqlResultMapDefinition.Create<TOut>(), _query, _connectionName);
    public SqlCommandValueProviderArgsBuilder<TIn, TOut> WithMapping<TOut>(Expression<Func<ISqlResultMapper, TOut>> expression)
        => new SqlCommandValueProviderArgsBuilder<TIn, TOut>(SqlResultMapDefinition.Create<TOut>(expression), _query, _connectionName);
}
public class SqlCommandValueProviderArgsBuilder<TIn, TOut>
{
    private SqlResultMapDefinition<TOut> _mapping;
    private readonly string _query;
    private readonly string _connectionName;
    public SqlCommandValueProviderArgsBuilder(SqlResultMapDefinition<TOut> mapping, string query, string connectionName)
        => (_mapping, _query, _connectionName) = (mapping, query, connectionName);
    internal SqlCommandValueProviderArgs<TIn, TOut> GetArgs()
        => new SqlCommandValueProviderArgs<TIn, TOut>
        {
            ConnectionName = _connectionName,
            Mapping = _mapping,
            SqlQuery = _query
        };
}
public class SqlCommandValueProviderArgs<TIn, TOut>
{
    public SqlResultMapDefinition<TOut> Mapping { get; set; }
    public string SqlQuery { get; set; }
    public string ConnectionName { get; set; }
}
public class SqlCommandValueProvider<TIn, TOut> : ValuesProviderBase<TIn, TOut>
{
    private readonly SqlCommandValueProviderArgs<TIn, TOut> _args;
    private static IDictionary<string, PropertyInfo> _inPropertyInfos = typeof(TIn).GetProperties().ToDictionary(i => i.Name, StringComparer.InvariantCultureIgnoreCase);
    public SqlCommandValueProvider(SqlCommandValueProviderArgs<TIn, TOut> args) => (_args) = (args);
    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    private TOut CreateRecord(IDataReader record, IList<SqlResultFieldDefinition> fieldDefinitions)
    {
        IDictionary<string, object> values = new Dictionary<string, object>();
        for (int i = 0; i < record.FieldCount; i++)
            values[record.GetName(i)] = record.GetValue(i) != DBNull.Value ? Convert.ChangeType(record.GetValue(i), record.GetFieldType(i)) : null;
        if (fieldDefinitions != null)
            values = fieldDefinitions.Join(values, l => l.ColumnName, r => r.Key, (l, r) => new { PropertyName = l.PropertyInfo.Name, r.Value }).ToDictionary(i => i.PropertyName, i => i.Value);
        return ObjectBuilder<TOut>.CreateInstance(values);
    }

    public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
    {
        // https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/ado-net-code-examples
        // https://stackoverflow.com/questions/5980615/parameterized-query-in-oracle-trouble
        var sqlConnection = _args.ConnectionName == null 
            ? context.Services.GetRequiredService<IDbConnection>() 
            : context.Services.GetRequiredKeyedService<IDbConnection>(_args.ConnectionName);
        var command = sqlConnection.CreateCommand();
        command.CommandText = _args.SqlQuery;
        command.CommandType = CommandType.Text;
        // var command = new SqlCommand(_args.SqlQuery, sqlConnection);
        Regex getParamRegex = new(@"@(?<param>\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var allMatches = getParamRegex.Matches(_args.SqlQuery).ToList().Select(match => match.Groups["param"].Value).Distinct().ToList();
        foreach (var parameterName in allMatches)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@{parameterName}";
            parameter.Value = _inPropertyInfos[parameterName].GetValue(input) ?? DBNull.Value;
            command.Parameters.Add(parameter);
            // command.Parameters.Add(new SqlParameter($"@{parameterName}", _inPropertyInfos[parameterName].GetValue(input) ?? DBNull.Value));
        }

        IList<SqlResultFieldDefinition> fieldDefinitions = _args.Mapping != null ? _args.Mapping.GetDefinitions() : null;
        using (var reader = command.ExecuteReader())
            while (reader.Read())
                push(CreateRecord(reader, fieldDefinitions));
    }
}
internal static class ConnectionEx
{
    public static IDbCommand CreateCommand(this IDbConnection sqlConnection, string sqlQuery, IDictionary<string, PropertyInfo> inPropertyInfos, object input)
    {
        var command = sqlConnection.CreateCommand();
        command.CommandText = sqlQuery;
        command.CommandType = CommandType.Text;
        // var command = new SqlCommand(_args.SqlQuery, sqlConnection);
        Regex getParamRegex = new Regex(@"@(?<param>\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var allMatches = getParamRegex.Matches(sqlQuery).ToList().Select(match => match.Groups["param"].Value).Distinct().ToList();
        foreach (var parameterName in allMatches)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@{parameterName}";
            parameter.Value = inPropertyInfos[parameterName].GetValue(input) ?? DBNull.Value;
            command.Parameters.Add(parameter);
            // command.Parameters.Add(new SqlParameter($"@{parameterName}", _inPropertyInfos[parameterName].GetValue(input) ?? DBNull.Value));
        }
        return command;
    }
}
