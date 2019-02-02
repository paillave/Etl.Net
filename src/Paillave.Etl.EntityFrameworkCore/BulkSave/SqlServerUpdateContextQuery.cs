using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Paillave.Etl.EntityFrameworkCore.BulkSave
{
    public class SqlServerUpdateContextQuery<T> : UpdateContextQueryBase<T>
    {
        private string SqlTargetTable => $"[{this.Schema}].[{this.Table}]";
        private string SqlStagingTableName => $"[{this.Schema}].[{this.Table}_temp_{this.StagingId}]";

        public SqlServerUpdateContextQuery(DbContext context, string schema, string table, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, IEntityType baseType, IDictionary<string, MemberInfo> propertiesGetter)
            : base(context, schema ?? "dbo", table, propertiesToUpdate, propertiesForPivot, propertiesToBulkLoad, baseType, propertiesGetter)
        {
        }

        protected virtual string CreateStagingTableSql()
            => $@"SELECT TOP 0 { string.Join(",", PropertiesToBulkLoad.Select(i => $"T.{i.SqlServer().ColumnName}")) }
                    INTO {SqlStagingTableName} FROM {SqlTargetTable} AS T 
                    LEFT JOIN {SqlTargetTable} AS Source ON 1 = 0;";

        public override void CreateStagingTable()
            => this.Context.Database.ExecuteSqlCommand(this.CreateStagingTableSql());

        public override void BulkSaveInStaging(IEnumerable<T> sources)
        {
            var sqlBulkCopy = new SqlBulkCopy(OpenAndGetSqlConnection(), SqlBulkCopyOptions.Default, null);
            sqlBulkCopy.DestinationTableName = SqlStagingTableName;
            sqlBulkCopy.BatchSize = sources.Count();
            foreach (var element in PropertiesToBulkLoad)
                sqlBulkCopy.ColumnMappings.Add(base.PropertyGetters[element.Name].Name, element.SqlServer().ColumnName);


            var dataReader = new ObjectDataReader(sources, new ObjectDataReaderConfig
            {
                EfProperties = PropertiesToBulkLoad,
                Types = new[] { base.BaseType },
                Context = Context,
                TempColumnNumOrderName = null
            });
            //sqlBulkCopy.EnableStreaming = true;
            sqlBulkCopy.WriteToServer(dataReader);
        }

        public override void IndexStagingTable()
            => this.Context.Database.ExecuteSqlCommand(this.IndexStagingTableSql());

        protected virtual string IndexStagingTableSql()
            => $@"CREATE UNIQUE CLUSTERED INDEX IX_{this.Table}_temp_{this.StagingId} ON {SqlStagingTableName} ({string.Join(", ", this.PropertiesForPivot.Select(i => i.SqlServer().ColumnName))})";

        public override void DeleteStagingTable()
            => this.Context.Database.ExecuteSqlCommand(this.DeleteStagingTableSql());

        protected virtual string DeleteStagingTableSql()
            => $@"IF OBJECT_ID('{SqlStagingTableName}', 'U') IS NOT NULL DROP TABLE {SqlStagingTableName}";


        protected SqlConnection OpenAndGetSqlConnection()
        {
            var connection = Context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) connection.Open();
            return (SqlConnection)connection;
        }

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
        private string CreateSetValueSql(string aliasRight, IProperty property)
            => $"{property.SqlServer().ColumnName} = {aliasRight}.{property.SqlServer().ColumnName}";

        protected virtual string MergeFromStagingSql()
        {
            return $@"UDPATE t
                    SET {string.Join(", ", this.PropertiesToUpdate.Select(i => CreateSetValueSql("s", i)))}
                    FROM {this.SqlStagingTableName} AS s
                    INNER JOIN {this.SqlTargetTable} AS t ON {string.Join(" AND ", this.PropertiesForPivot.Select(i => CreateEqualityConditionSql("s", "t", i)))}";
        }
    }
}
