using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave.SqlServer;

public class SqlServerSaveContextQuery<T>(DbContext Context, string? schema, string table, List<IProperty> propertiesToInsert, List<IProperty> propertiesToUpdate, List<List<IProperty>> propertiesForPivotSet, List<IProperty> propertiesToBulkLoad, List<IEntityType> entityTypes, CancellationToken cancellationToken, StoreObjectIdentifier storeObject) : SaveContextQueryBase<T>(Context, schema ?? "dbo", table, propertiesToInsert, propertiesToUpdate, propertiesForPivotSet, propertiesToBulkLoad, entityTypes, cancellationToken, storeObject) where T : class
{
    private const string TempColumnNumOrderName = "_TempColumnNumOrder";
    private const string TempColumnAction = "_Action";
    private string SqlTargetTable => $"[{this.Schema}].[{this.Table}]";
    private string SqlStagingTableName => $"[{this.Schema}].[{this.Table}_temp_{this.StagingId}]";
    private string SqlOutputStagingTableName => $"[{this.Schema}].[{this.Table}_tempoutput_{this.StagingId}]";

    public override int CreateStagingTable()
        => this.Context.Database.ExecuteSqlRaw(this.CreateStagingTableSql());

    protected virtual string CreateStagingTableSql()
        => $@"SELECT TOP 0 {string.Join(",", PropertiesToBulkLoad.Select(i => $"T.[{i.GetColumnName(base.StoreObject)}]"))}, 0 as [{TempColumnNumOrderName}]
                    INTO {SqlStagingTableName} FROM {SqlTargetTable} AS T 
                    LEFT JOIN {SqlTargetTable} AS Source ON 1 = 0 option(recompile);";

    public override void IndexStagingTable(List<IProperty> propertiesForPivot)
    {
        this.Context.Database.ExecuteSqlRaw(this.IndexStagingTableSql(propertiesForPivot));
        this.Context.Database.ExecuteSqlRaw(this.CounterIndexStagingTableSql(propertiesForPivot));
    }

    protected virtual string IndexStagingTableSql(List<IProperty> propertiesForPivot)
        => $@"CREATE UNIQUE CLUSTERED INDEX IX_{this.Table}_temp_{this.StagingId}_PivotKey ON {SqlStagingTableName} ({string.Join(", ", propertiesForPivot.Select(i => $"[{i.GetColumnName(base.StoreObject)}]"))})";

    protected virtual string CounterIndexStagingTableSql(List<IProperty> propertiesForPivot)
        => $@"CREATE UNIQUE INDEX IX_{this.Table}_temp_{this.StagingId}_{TempColumnNumOrderName} ON {SqlStagingTableName} ({TempColumnNumOrderName})";

    public override void DeleteStagingTable()
        => this.Context.Database.ExecuteSqlRaw(this.DeleteStagingTableSql());

    protected virtual string DeleteStagingTableSql()
        => $@"IF OBJECT_ID('{SqlStagingTableName}', 'U') IS NOT NULL DROP TABLE {SqlStagingTableName}";

    public override void DeleteOutputStagingTable()
        => this.Context.Database.ExecuteSqlRaw(this.DeleteOutputStagingTableSql());

    protected virtual string DeleteOutputStagingTableSql()
        => $@"IF OBJECT_ID('{SqlOutputStagingTableName}', 'U') IS NOT NULL DROP TABLE {SqlOutputStagingTableName}";

    public override int CreateOutputStagingTable()
        => this.Context.Database.ExecuteSqlRaw(this.CreateOutputStagingTableSql());

    protected virtual string CreateOutputStagingTableSql()
        => $@"SELECT TOP 0 T.*, 0 as [{TempColumnNumOrderName}], 'mergeaction' as [{TempColumnAction}]
                    INTO {SqlOutputStagingTableName} FROM {SqlTargetTable} AS T 
                    LEFT JOIN {SqlTargetTable} AS Source ON 1 = 0 option(recompile);";

    public override void IndexOutputStagingTable()
        => this.Context.Database.ExecuteSqlRaw(this.IndexOutputStagingTableSql());

    protected virtual string IndexOutputStagingTableSql()
        => $@"CREATE UNIQUE CLUSTERED INDEX IX_{this.Table}_tempoutput_{this.StagingId} ON {SqlOutputStagingTableName} ([{TempColumnNumOrderName}])";

    public override void BulkSaveInStaging(IEnumerable<T> entities)
    {
        var sqlBulkCopy = new SqlBulkCopy(OpenAndGetSqlConnection(), SqlBulkCopyOptions.Default, null);
        sqlBulkCopy.DestinationTableName = SqlStagingTableName;
        sqlBulkCopy.BulkCopyTimeout = 300;
        sqlBulkCopy.BatchSize = entities.Count();
        foreach (var element in PropertiesToBulkLoad)
            sqlBulkCopy.ColumnMappings.Add(element.Name, element.GetColumnName(base.StoreObject));
        sqlBulkCopy.ColumnMappings.Add(TempColumnNumOrderName, TempColumnNumOrderName);


        var dataReader = new ObjectDataReader(entities, new ObjectDataReaderConfig
        {
            EfProperties = PropertiesToBulkLoad,
            Types = EntityTypes,
            Context = Context,
            TempColumnNumOrderName = TempColumnNumOrderName
        });
        // sqlBulkCopy.SqlRowsCopied += (sender, eventArgs) =>
        // {
        //     Console.WriteLine("Wrote " + eventArgs.RowsCopied + " records.");
        // };
        sqlBulkCopy.WriteToServer(dataReader);
    }
    protected SqlConnection OpenAndGetSqlConnection()
    {
        var connection = Context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) connection.Open();
        return (SqlConnection)connection;
    }

    public override DataTable GetOutputStaging()
        => base.StrictlyExecuteSql(GetOutputStagingSql());

    public override DataTable GetOutputStagingForComputedColumns()
        => base.StrictlyExecuteSql(GetOutputStagingForComputedColumnsSql());


    protected virtual string GetOutputStagingSql()
        => $@"select * from {SqlOutputStagingTableName} order by [{TempColumnNumOrderName}] option(recompile)"; //the sp_execute is to prevent EF core to wrap the query into another subquery that will involve a different sorting depending in the situtation (not too proud of this solution, but I really didn't find better)
                                                                                                                // => $@"sp_executesql N'set nocount on; select * from {SqlOutputStagingTableName} order by {TempColumnNumOrderName}';"; //the sp_execute is to prevent EF core to wrap the query into another subquery that will involve a different sorting depending in the situtation (not too proud of this solution, but I really didn't find better)


    protected virtual string GetOutputStagingForComputedColumnsSql()
        => $@"select T.* from {SqlStagingTableName} as S left join {SqlTargetTable} as T on {string.Join(" OR ", this.PropertiesForPivotSet.Select(propertiesForPivot => string.Join(" AND ", propertiesForPivot.Select(i => CreateEqualityConditionSql("T", "S", i)))))} order by S.{TempColumnNumOrderName} option(recompile)";

    public override void MergeFromStaging(bool doNotUpdateIfExists = false)
        => this.Context.Database.ExecuteSqlRaw(this.MergeFromStagingSql(doNotUpdateIfExists));

    public override void InsertFromStaging()
        => this.Context.Database.ExecuteSqlRaw(this.InsertFromStagingSql());

    private string CreateEqualityConditionSql(string aliasLeft, string aliasRight, IProperty property)
    {
        string regularEquality = $"{aliasLeft}.[{property.GetColumnName(base.StoreObject)}] = {aliasRight}.[{property.GetColumnName(base.StoreObject)}]";
        if (property.IsColumnNullable())
            return $"{aliasRight}.[{property.GetColumnName(base.StoreObject)}] is not null and {regularEquality}";
        else
            return regularEquality;
    }
    private string BuildPivotCriteria()
    {
        return string.Join(" OR ", PropertiesForPivotSet.Select(this.BuildUnitPivotCriteria));
    }
    private string BuildUnitPivotCriteria(List<IProperty> propertiesForPivot)
    {
        return "(" + string.Join(" AND ", propertiesForPivot.Select(i => CreateEqualityConditionSql("T", "S", i))) + ")";
    }
    private string GetWhenMatchedMergeStatement(HashSet<string> pivotColumns, bool doNotUpdateIfExists)
    {
        if (PropertiesToUpdate.Count == 0)
            return "";
        if (doNotUpdateIfExists)
        {
            return $@"WHEN MATCHED THEN 
                    UPDATE SET {string.Join(", ", PropertiesToUpdate.Select(i => i.GetColumnName(base.StoreObject)).Select(columnName => $"T.[{columnName}] = T.[{columnName}]").ToArray())}";
        }
        return $@"WHEN MATCHED THEN 
                    UPDATE SET {string.Join(", ", PropertiesToUpdate.Select(i =>
        {
            var columnName = i.GetColumnName(base.StoreObject);
            if (string.IsNullOrWhiteSpace(columnName))
                throw new Exception("Column name is empty");
            if (pivotColumns.Contains(columnName) && i.IsColumnNullable())
                return $"T.[{columnName}] = ISNULL(T.[{columnName}], S.[{columnName}])";
            else
                return $"T.[{columnName}] = S.[{columnName}]";
        }))}";
    }
    protected virtual string MergeFromStagingSql(bool doNotUpdateIfExists = false)
    {
        var pivotColumns = PropertiesForPivotSet.SelectMany(p => p.Select(i => i.GetColumnName(base.StoreObject)).Where(i => !string.IsNullOrWhiteSpace(i)).Cast<string>()).ToHashSet();
        string whenNotMatchedStatement = $@"WHEN NOT MATCHED BY TARGET THEN 
                    INSERT ({string.Join(", ", PropertiesToInsert.Select(i => $"[{i.GetColumnName(base.StoreObject)}]"))})
                    VALUES ({string.Join(", ", PropertiesToInsert.Select(i => $"S.[{i.GetColumnName(base.StoreObject)}]"))})";
        string whenMatchedStatement = this.GetWhenMatchedMergeStatement(pivotColumns, doNotUpdateIfExists);
        string pivotCriteria = this.BuildPivotCriteria();
        return $@"MERGE {SqlTargetTable} AS T 
                    USING {SqlStagingTableName} AS S 
                    ON {pivotCriteria}
                    {whenNotMatchedStatement}
                    {whenMatchedStatement}
                    OUTPUT INSERTED.*, S.[{TempColumnNumOrderName}], $action
                    INTO {SqlOutputStagingTableName} option(recompile);";
    }
    protected virtual string InsertFromStagingSql()
    {
        string whenNotMatchedStatement = $@"WHEN NOT MATCHED BY TARGET THEN 
                    INSERT ({string.Join(", ", PropertiesToInsert.Select(i => $"[{i.GetColumnName(base.StoreObject)}]"))})
                    VALUES ({string.Join(", ", PropertiesToInsert.Select(i => $"S.[{i.GetColumnName(base.StoreObject)}]"))})";
        return $@"MERGE {SqlTargetTable} AS T 
                    USING {SqlStagingTableName} AS S 
                    ON 1=0
                    {whenNotMatchedStatement}
                    OUTPUT INSERTED.*, S.[{TempColumnNumOrderName}], $action
                    INTO {SqlOutputStagingTableName} option(recompile);";
    }
}
