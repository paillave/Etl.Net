using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Paillave.Etl.EntityFrameworkCore.BulkSave
{
    public class SqlServerSaveContextQuery<T> : SaveContextQueryBase<T> where T : class
    {
        private const string TempColumnNumOrderName = "_TempColumnNumOrder";
        private string SqlTargetTable => $"[{this.Schema}].[{this.Table}]";
        private string SqlStagingTableName => $"[{this.Schema}].[{this.Table}_temp_{this.StagingId}]";
        private string SqlOutputStagingTableName => $"[{this.Schema}].[{this.Table}_tempoutput_{this.StagingId}]";

        public SqlServerSaveContextQuery(DbContext Context, string schema, string table, List<IProperty> propertiesToInsert, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, List<IEntityType> entityTypes)
            : base(Context, schema ?? "dbo", table, propertiesToInsert, propertiesToUpdate, propertiesForPivot, propertiesToBulkLoad, entityTypes)
        {
        }

        public override void CreateStagingTable()
            => this.Context.Database.ExecuteSqlCommand(this.CreateStagingTableSql());

        protected virtual string CreateStagingTableSql()
            => $@"SELECT TOP 0 { string.Join(",", PropertiesToBulkLoad.Select(i => $"T.{i.SqlServer().ColumnName}")) }, 0 as {TempColumnNumOrderName}
                    INTO {SqlStagingTableName} FROM {SqlTargetTable} AS T 
                    LEFT JOIN {SqlTargetTable} AS Source ON 1 = 0;";

        public override void IndexStagingTable()
            => this.Context.Database.ExecuteSqlCommand(this.IndexStagingTableSql());

        protected virtual string IndexStagingTableSql()
            => $@"CREATE UNIQUE CLUSTERED INDEX IX_{this.Table}_temp_{this.StagingId} ON {SqlStagingTableName} ({string.Join(", ", this.PropertiesForPivot.Select(i => i.SqlServer().ColumnName))})";

        public override void DeleteStagingTable()
            => this.Context.Database.ExecuteSqlCommand(this.DeleteStagingTableSql());

        protected virtual string DeleteStagingTableSql()
            => $@"IF OBJECT_ID('{SqlStagingTableName}', 'U') IS NOT NULL DROP TABLE {SqlStagingTableName}";

        public override void DeleteOutputStagingTable()
            => this.Context.Database.ExecuteSqlCommand(this.DeleteOutputStagingTableSql());

        protected virtual string DeleteOutputStagingTableSql()
            => $@"IF OBJECT_ID('{SqlOutputStagingTableName}', 'U') IS NOT NULL DROP TABLE {SqlOutputStagingTableName}";

        public override void CreateOutputStagingTable()
            => this.Context.Database.ExecuteSqlCommand(this.CreateOutputStagingTableSql());

        protected virtual string CreateOutputStagingTableSql()
            => $@"SELECT TOP 0 T.*, 0 as {TempColumnNumOrderName}
                    INTO {SqlOutputStagingTableName} FROM {SqlTargetTable} AS T 
                    LEFT JOIN {SqlTargetTable} AS Source ON 1 = 0;";

        public override void IndexOutputStagingTable()
            => this.Context.Database.ExecuteSqlCommand(this.IndexOutputStagingTableSql());

        protected virtual string IndexOutputStagingTableSql()
            => $@"CREATE UNIQUE CLUSTERED INDEX IX_{this.Table}_tempoutput_{this.StagingId} ON {SqlOutputStagingTableName} ({TempColumnNumOrderName})";

        public override void BulkSaveInStaging(IEnumerable<T> entities)
        {
            var sqlBulkCopy = new SqlBulkCopy(OpenAndGetSqlConnection(), SqlBulkCopyOptions.Default, null);
            sqlBulkCopy.DestinationTableName = SqlStagingTableName;
            sqlBulkCopy.BatchSize = entities.Count();
            foreach (var element in PropertiesToBulkLoad)
                sqlBulkCopy.ColumnMappings.Add(element.Name, element.SqlServer().ColumnName);
            sqlBulkCopy.ColumnMappings.Add(TempColumnNumOrderName, TempColumnNumOrderName);


            var dataReader = new ObjectDataReader(entities, new ObjectDataReaderConfig
            {
                EfProperties = PropertiesToBulkLoad,
                Types = EntityTypes,
                Context = Context,
                TempColumnNumOrderName = TempColumnNumOrderName
            });
            sqlBulkCopy.WriteToServer(dataReader);
        }
        protected SqlConnection OpenAndGetSqlConnection()
        {
            var connection = Context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) connection.Open();
            return (SqlConnection)connection;
        }

        public override IList<T> GetOutputStaging()
            => base.QueryOutputTable(GetOutputStagingSql()).ToList();

        public override IList<T> GetOutputStagingForComputedColumns()
            => base.QueryOutputTable(GetOutputStagingForComputedColumnsSql()).ToList();


        protected virtual string GetOutputStagingSql()
            => $@"sp_executesql N'set nocount on; select * from {SqlOutputStagingTableName} order by {TempColumnNumOrderName}';"; //the sp_execute is to prevent EF core to wrap the query into another subquery that will involve a different sorting depending in the situtation (not too proud of this solution, but I really didn't find better)


        protected virtual string GetOutputStagingForComputedColumnsSql()
            => $@"sp_executesql N'set nocount on; select T.* from {SqlStagingTableName} as S left join {SqlTargetTable} as T on {string.Join(" AND ", PropertiesForPivot.Select(i => CreateEqualityConditionSql("T", "S", i)))} order by S.{TempColumnNumOrderName}';";

        public override void MergeFromStaging()
            => this.Context.Database.ExecuteSqlCommand(this.MergeFromStagingSql());

        private string CreateEqualityConditionSql(string aliasLeft, string aliasRight, IProperty property)
        {
            string regularEquality = $"{aliasLeft}.{property.SqlServer().ColumnName} = {aliasRight}.{property.SqlServer().ColumnName}";
            if (property.IsColumnNullable())
                return $"({aliasLeft}.{property.SqlServer().ColumnName} is null and {regularEquality})";
            else
                return regularEquality;
        }
        protected virtual string MergeFromStagingSql()
        {
            string whenNotMatchedStatement = $@"WHEN NOT MATCHED BY TARGET THEN 
                    INSERT ({string.Join(", ", PropertiesToInsert.Select(i => i.SqlServer().ColumnName))})
                    VALUES ({string.Join(", ", PropertiesToInsert.Select(i => $"S.{i.SqlServer().ColumnName}"))})";
            string whenMatchedStatement = PropertiesToUpdate.Count == 0 ? "" : $@"WHEN MATCHED THEN 
                    UPDATE SET {string.Join(", ", PropertiesToUpdate.Select(i => $"T.{i.SqlServer().ColumnName} = S.{i.SqlServer().ColumnName}"))}";
            return $@"MERGE {SqlTargetTable} WITH (HOLDLOCK) AS T 
                    USING {SqlStagingTableName} AS S 
                    ON {string.Join(" AND ", PropertiesForPivot.Select(i => CreateEqualityConditionSql("T", "S", i)))}
                    {whenNotMatchedStatement}
                    {whenMatchedStatement}
                    OUTPUT INSERTED.*, S.{TempColumnNumOrderName}
                    INTO {SqlOutputStagingTableName};";
        }
    }
}
