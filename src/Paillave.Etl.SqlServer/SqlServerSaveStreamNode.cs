using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Paillave.Etl.SqlServer;


public class SqlServerSaveCommandArgsBuilder<TIn, TValue>(Func<TIn, TValue> GetValue) where TIn : class
{
    internal string? ConnectionName { get; private set; }
    internal string Table { get; private set; } = typeof(TValue).Name;
    internal Expression<Func<TValue, object>>? Pivot { get; private set; } = null;
    internal Expression<Func<TValue, object>>? Computed { get; private set; } = null;

    public SqlServerSaveCommandArgsBuilder<TIn, TValue> ToTable(string table)
    {
        Table = table;
        return this;
    }
    public SqlServerSaveCommandArgsBuilder<TIn, TValue> SeekOn(Expression<Func<TValue, object>> pivot)
    {
        Pivot = pivot;
        return this;
    }
    public SqlServerSaveCommandArgsBuilder<TIn, TValue> DoNotSave(Expression<Func<TValue, object>> computed)
    {
        Computed = computed;
        return this;
    }
    public SqlServerSaveCommandArgsBuilder<TIn, TValue> WithConnection(string connectionName)
    {
        ConnectionName = connectionName;
        return this;
    }

    internal SqlServerSaveCommandArgs<TIn, TStream, TValue> GetArgs<TStream>(TStream sourceStream) where TStream : IStream<TIn>
        => new(sourceStream, GetValue, Table, Pivot, Computed, ConnectionName);
}


public record SqlServerSaveCommandArgs<TIn, TStream, TValue>(TStream SourceStream,
                                                             Func<TIn, TValue> GetValue,
                                                             string Table,
                                                             Expression<Func<TValue, object>>? Pivot,
                                                             Expression<Func<TValue, object>>? Computed,
                                                             string? ConnectionName
                                                             ) where TIn : class where TStream : IStream<TIn>;


public partial class SqlServerSaveStreamNode<TIn, TStream, TValue>(string name, SqlServerSaveCommandArgs<TIn, TStream, TValue> args) : StreamNodeBase<TIn, TStream, SqlServerSaveCommandArgs<TIn, TStream, TValue>>(name, args)
    where TIn : class
    where TStream : IStream<TIn>
{
    private static readonly IDictionary<string, PropertyInfo> _inPropertyInfos = typeof(TIn).GetProperties().ToDictionary(i => i.Name, StringComparer.InvariantCultureIgnoreCase);
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override TStream CreateOutputStream(SqlServerSaveCommandArgs<TIn, TStream, TValue> args)
    {
        var ret = args.SourceStream.Observable.Do(i => ProcessItem(args.GetValue(i), args.ConnectionName));
        return base.CreateMatchingStream(ret, args.SourceStream);
    }

    private string _sqlStatement = null!;
    private List<PropertyInfo> _pivot = null!;
    private List<PropertyInfo> _computed = null!;
    private bool _usePositionalParameters = false;
    private List<string> _positionalParamsMap = [];


    private string GetSqlStatement()
    {
        if (_sqlStatement == null)
        {
            _pivot = base.Args.Pivot == null ? new List<PropertyInfo>() : base.Args.Pivot.GetPropertyInfos();
            _computed = base.Args.Computed == null ? new List<PropertyInfo>() : base.Args.Computed.GetPropertyInfos();
            _sqlStatement = CreateSqlQuery(Args.Table, _inPropertyInfos.Values.Except(_computed), _pivot);
            if (_usePositionalParameters)
                (_sqlStatement, _positionalParamsMap) = AdjustQueryForPositionalParameters(_sqlStatement);
        }
        return _sqlStatement;
    }


    private void ProcessItem(TValue item, string? connectionName)
    {
        using var sqlConnection = connectionName == null
                                ? this.ExecutionContext.Services.GetRequiredService<IDbConnection>() 
                                : this.ExecutionContext.Services.GetRequiredKeyedService<IDbConnection>(connectionName);
        if (sqlConnection.State != ConnectionState.Open)
            sqlConnection.Open();

        _usePositionalParameters = sqlConnection is OdbcConnection or OleDbConnection;
        var sqlStatement = GetSqlStatement();
        var command = sqlConnection.CreateCommand();
        command.CommandText = sqlStatement;
        command.CommandType = CommandType.Text;

        //Positional parameters must adhere to their position in the SQL statement, including repeat uses.
        //Otherwise just use all relevant properties of the item.
        var parameterNames = _usePositionalParameters
                           ? _positionalParamsMap
                           : _inPropertyInfos.Keys.Except(_computed.Select(i => i.Name));

        foreach (var parameterName in parameterNames)
            command.Parameters.Add(MakeParameterFromPropertyname(command, item, parameterName));

        using var reader = command.ExecuteReader();
        if (reader.Read())
            UpdateItem(item, reader);
    }


    private static IDbDataParameter MakeParameterFromPropertyname(IDbCommand command, TValue item, string parameterName)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = $"@{parameterName}";
        parameter.Value = _inPropertyInfos[parameterName].GetValue(item) ?? DBNull.Value;
        if (_inPropertyInfos[parameterName].PropertyType == typeof(byte[]))
            parameter.DbType = DbType.Binary;
        return parameter;
    }


    private void UpdateItem(TValue item, IDataReader record)
    {
        for (int i = 0; i < record.FieldCount; i++)
        {
            var val = record.GetValue(i);
            val = val == DBNull.Value ? null : Convert.ChangeType(val, record.GetFieldType(i));
            if (_inPropertyInfos.TryGetValue(record.GetName(i), out var prop) && prop.GetSetMethod() is not null)
                prop.SetValue(item, val);
        }
    }


    private string CreateSqlQuery(string table, IEnumerable<PropertyInfo> upsertProperties, IEnumerable<PropertyInfo> matchProperties)
    {
        var upsertProps = upsertProperties.ToList();
        var matchProps = matchProperties.ToList();

        var insert = $"""
            insert into {table} ({string.Join(", ", upsertProps.Select(o => $"[{o.Name}]"))})
            output inserted.*
            values ({string.Join(", ", upsertProps.Select(i => $"@{i.Name}"))})
            """;

        if (matchProps.Count == 0)
            return insert;

        var pivotCondition = string.Join(" and ", matchProps.Select(p => $"p.[{p.Name}] = @{p.Name}"));
        var setStatement = string.Join(", ", upsertProps.Except(matchProps).Select(i => $"[{i.Name}] = @{i.Name}"));
        var query = $"""
            if (exists(select 1 from {table} as p where {pivotCondition}))
                update p
                set {setStatement}
                output inserted.*
                from {table} as p where {pivotCondition}
            else
                {insert}
            """;

        return query;
    }


    [GeneratedRegex(@"@(\w+)")]
    private static partial Regex ParameterRegex();
    /// <summary>
    /// Converts parameters to <c>?</c> and notes their order for later in <see cref="_positionalParamsMap"/>.
    /// </summary>
    private static (string Query, List<string> PositionalParameterOrder) AdjustQueryForPositionalParameters(string query)
    {
        var parameters = new List<string>();
        var newQuery = ParameterRegex().Replace(query, m =>
        {
            parameters.Add(m.Groups[1].Value);
            return "?";
        });
        return (newQuery, parameters);
    }
}
