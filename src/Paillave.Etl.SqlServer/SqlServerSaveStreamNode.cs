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
    private string? ConnectionName;
    private string Table = typeof(TValue).Name;
    private Expression<Func<TValue, object>>? Pivot;
    private Expression<Func<TValue, object>>? Computed;
    private bool ReadBack = false;

    /// <summary>
    /// The name of the table to which items will be saved, including schema as necessary. If omitted, <see cref="TValue"/>’s type name will be used.
    /// </summary>
    /// <remarks>
    /// This string is used verbatim in generated SQL and cannot be parameterized.
    /// <strong>This is an SQL injection vector. Validate and escape untrusted input appropriately.</strong>
    /// </remarks>
    public SqlServerSaveCommandArgsBuilder<TIn, TValue> ToTable(string table)
    {
        Table = table;
        return this;
    }
    /// <summary>
    /// Properties specified here will be used to match existing database rows to update rather than insert (“upsert”).
    /// </summary>
    public SqlServerSaveCommandArgsBuilder<TIn, TValue> SeekOn(Expression<Func<TValue, object>> pivot)
    {
        Pivot = pivot;
        return this;
    }
    /// <summary>
    /// Use this to exclude properties from the upsert. E.g. database-generated columns like ids and timestamps, or properties with no associated column.
    /// </summary>
    public SqlServerSaveCommandArgsBuilder<TIn, TValue> DoNotSave(Expression<Func<TValue, object>> propertiesToExclude)
    {
        Computed = propertiesToExclude;
        return this;
    }
    /// <summary>
    /// Request rows back from the database and apply changes to the processed items.
    /// </summary>
    public SqlServerSaveCommandArgsBuilder<TIn, TValue> ReadBackChanges()
    {
        if (typeof(TValue).IsValueType)
            throw new InvalidOperationException($"Type {typeof(TValue).Name} must be a reference type if {nameof(ReadBackChanges)}() is used.");

        ReadBack = true;
        return this;
    }
    /// <summary>
    /// Service key of the <see cref="IDbConnection"/> to use. <strong>This functionality is currently broken!</strong>
    /// </summary>
    [Obsolete("Keyed Services are not implemented yet. Using this will currently throw.", error: true)]
    public SqlServerSaveCommandArgsBuilder<TIn, TValue> WithConnection(string connectionName)
    {
        ConnectionName = connectionName;
        return this;
    }

    internal SqlServerSaveCommandArgs<TIn, TStream, TValue> GetArgs<TStream>(TStream sourceStream) where TStream : IStream<TIn>
        => new(sourceStream, GetValue, Table, Pivot, Computed, ConnectionName, ReadBack);
}


public record SqlServerSaveCommandArgs<TIn, TStream, TValue>(TStream SourceStream,
                                                             Func<TIn, TValue> GetValue,
                                                             string Table,
                                                             Expression<Func<TValue, object>>? Pivot,
                                                             Expression<Func<TValue, object>>? Computed,
                                                             string? ConnectionName,
                                                             bool ReadBackChanges
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
        var ret = args.SourceStream.Observable.Do(i => ProcessItem(args.GetValue(i)));
        return base.CreateMatchingStream(ret, args.SourceStream);
    }

    private bool _isInitialized = false;
    private IDbConnection? _sqlConnection;
    private string? _sqlStatement;
    private List<string>? _paramPropertyNames;
    

    private void ProcessItem(TValue item)
    {
        if (!_isInitialized)
            Initialize();

        if (_sqlConnection!.State != ConnectionState.Open)
            _sqlConnection.Open();

        using var command = _sqlConnection.CreateCommand();
        command.CommandText = _sqlStatement;

        foreach (var parameterName in _paramPropertyNames!)
            command.Parameters.Add(MakeParameterFromPropertyname(command, item, parameterName));

        if (!Args.ReadBackChanges)
        {
            command.ExecuteNonQuery();
        }
        else
        {
            using var reader = command.ExecuteReader();
            if (reader.Read() || (reader.NextResult() && reader.Read()))
                UpdateItem(item, reader);
        }
    }

    
    private void Initialize()
    {
        _sqlConnection = args.ConnectionName == null
                       ? this.ExecutionContext.Services.GetRequiredService<IDbConnection>()
                       : this.ExecutionContext.Services.GetRequiredKeyedService<IDbConnection>(args.ConnectionName);
        this.ExecutionContext.AddDisposable(_sqlConnection);

        var usePositionalParameters = _sqlConnection is OdbcConnection or OleDbConnection;
        var pivotParameters = Args.Pivot == null ? [] : Args.Pivot.GetPropertyInfos();
        var computedProperties = Args.Computed == null ? [] : Args.Computed.GetPropertyInfos();
        var upsertProperties = _inPropertyInfos.Values.Except(computedProperties).ToList();
        _sqlStatement = CreateSqlQuery(Args.Table, upsertProperties, pivotParameters, Args.ReadBackChanges);

        //Positional parameters must adhere to their position in the SQL statement, including repeat uses.
        //Otherwise just use all relevant properties of the item.
        if (usePositionalParameters)
            (_sqlStatement, _paramPropertyNames) = AdjustQueryForPositionalParameters(_sqlStatement);
        else
            _paramPropertyNames = upsertProperties.Union(pivotParameters).Select(i => i.Name).ToList();

        _isInitialized = true;
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


    private static void UpdateItem(TValue item, IDataReader record)
    {
        for (int i = 0; i < record.FieldCount; i++)
        {
            var val = record.GetValue(i);
            val = val == DBNull.Value ? null : Convert.ChangeType(val, record.GetFieldType(i));
            if (_inPropertyInfos.TryGetValue(record.GetName(i), out var prop) && prop.GetSetMethod() is not null)
                prop.SetValue(item, val);
        }
    }


    private static string CreateSqlQuery(string table, IEnumerable<PropertyInfo> upsertProperties, IEnumerable<PropertyInfo> matchProperties, bool readBackChanges)
    {
        var upsertProps = upsertProperties.ToList();
        var matchProps = matchProperties.ToList();
        var outputClause = readBackChanges ? "output inserted.*" : "";

        if (upsertProps.Count == 0)
            throw new InvalidOperationException($"No properties to save were found on type {typeof(TIn).Name} after excluding those in {nameof(SqlServerSaveCommandArgsBuilder<,>.DoNotSave)}().");

        var insert = $"""
            insert into {table} ({string.Join(", ", upsertProps.Select(o => $"[{o.Name}]"))})
            {outputClause}
            values ({string.Join(", ", upsertProps.Select(i => $"@{i.Name}"))})
            """;

        if (matchProps.Count == 0)
            return insert;

        var updateProps = upsertProps.Except(matchProps).ToList();
        if (updateProps.Count == 0)
            throw new InvalidOperationException($"No properties to save were found on type {typeof(TIn).Name} after excluding those in {nameof(SqlServerSaveCommandArgsBuilder<,>.DoNotSave)}() and {nameof(SqlServerSaveCommandArgsBuilder<,>.SeekOn)}().");

        var pivotCondition = string.Join(" and ", matchProps.Select(p => $"([{p.Name}] = @{p.Name} or ([{p.Name}] is null and @{p.Name} is null))"));
        var setStatement = string.Join(", ", updateProps.Select(i => $"[{i.Name}] = @{i.Name}"));
        var query = $"""
            update top(1) {table} with (updlock, serializable)
            set {setStatement}
            {outputClause}
            where {pivotCondition}
            
            if (@@ROWCOUNT = 0)
            begin
                {insert}
            end
            """;

        return query;
    }


    [GeneratedRegex(@"(?<!@)@(\w+)")]
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
