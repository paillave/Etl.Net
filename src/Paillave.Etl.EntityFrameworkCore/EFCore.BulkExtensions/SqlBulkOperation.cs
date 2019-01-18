using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastMember;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.BulkExtensions
{
    internal static class SqlBulkOperation
    {
        internal static string ColumnMappingExceptionMessage => "The given ColumnMapping does not match up with any column in the source or destination";

        #region MainOps
        public static void Insert<T>(DbContext context, IList<T> entities, TableInfo tableInfo) where T : class
        {
            var sqlConnection = OpenAndGetSqlConnection(context);
            var transaction = context.Database.CurrentTransaction;
            try
            {
                using (var sqlBulkCopy = GetSqlBulkCopy(sqlConnection, transaction, tableInfo.BulkConfig))
                {
                    tableInfo.SetSqlBulkCopyConfig(sqlBulkCopy, entities);
                    try
                    {
                        using (var reader = ObjectReaderEx<T>.Create(entities, tableInfo.ShadowProperties, tableInfo.ConvertibleProperties, context, tableInfo.PropertyColumnNamesDict.Keys.ToArray()))
                        {
                            var previousAutoDetect = context.ChangeTracker.AutoDetectChangesEnabled;
                            context.ChangeTracker.AutoDetectChangesEnabled = false;
                            sqlBulkCopy.WriteToServer(reader);
                            context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (!tableInfo.BulkConfig.UseTempDB)
                            context.Database.ExecuteSqlCommand(SqlQueryBuilder.DropTable(tableInfo.FullTempOutputTableName));
                        if (ex.Message.Contains(ColumnMappingExceptionMessage))
                            context.Database.ExecuteSqlCommand(SqlQueryBuilder.CreateTableCopy(tableInfo.FullTableName, tableInfo.FullTempTableName, tableInfo)); // Will throw Exception specify missing db column: Invalid column name ''
                        throw ex;
                    }
                }
            }
            finally
            {
                if (transaction == null)
                {
                    sqlConnection.Close();
                }
            }
        }

        public static async Task InsertAsync<T>(DbContext context, IList<T> entities, TableInfo tableInfo) where T : class
        {
            var sqlConnection = await OpenAndGetSqlConnectionAsync(context);
            var transaction = context.Database.CurrentTransaction;
            try
            {
                using (var sqlBulkCopy = GetSqlBulkCopy(sqlConnection, transaction, tableInfo.BulkConfig))
                {
                    tableInfo.SetSqlBulkCopyConfig(sqlBulkCopy, entities);
                    try
                    {
                        using (var reader = ObjectReaderEx<T>.Create(entities, tableInfo.ShadowProperties, tableInfo.ConvertibleProperties, context, tableInfo.PropertyColumnNamesDict.Keys.ToArray()))
                        {
                            var currentAutoDetectChangesEnabled = context.ChangeTracker.AutoDetectChangesEnabled;
                            context.ChangeTracker.AutoDetectChangesEnabled = false;
                            await sqlBulkCopy.WriteToServerAsync(reader)
                                .ContinueWith(t => context.ChangeTracker.AutoDetectChangesEnabled = currentAutoDetectChangesEnabled)
                                .ConfigureAwait(false);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (!tableInfo.BulkConfig.UseTempDB)
                        {
                            await context.Database.ExecuteSqlCommandAsync(SqlQueryBuilder.DropTable(tableInfo.FullTempOutputTableName));
                        }
                        if (ex.Message.Contains(ColumnMappingExceptionMessage))
                        {
                            await context.Database.ExecuteSqlCommandAsync(SqlQueryBuilder.CreateTableCopy(tableInfo.FullTableName, tableInfo.FullTempTableName, tableInfo));
                        }
                        throw ex;
                    }
                }
            }
            finally
            {
                if (transaction == null)
                {
                    sqlConnection.Close();
                }
            }
        }

        public static void Merge<T>(DbContext context, IList<T> entities, TableInfo tableInfo) where T : class
        {
            tableInfo.InsertToTempTable = true;

            context.Database.ExecuteSqlCommand(SqlQueryBuilder.CreateTableCopy(tableInfo.FullTableName, tableInfo.FullTempTableName, tableInfo));

            bool keepIdentity = tableInfo.BulkConfig.SqlBulkCopyOptions.HasFlag(SqlBulkCopyOptions.KeepIdentity);
            try
            {
                Insert(context, entities, tableInfo);

                if (keepIdentity && tableInfo.HasIdentityColumn)
                {
                    context.Database.OpenConnection();
                    context.Database.ExecuteSqlCommand(SqlQueryBuilder.SetIdentityInsert(tableInfo.FullTableName, true));
                }

                string query = SqlQueryBuilder.MergeTable(tableInfo);

                var tst = tableInfo.QueryOutputTable<T>(context, query).ToList();
                tableInfo.UpdateEntitiesIdentityIfNeeded<T>(entities, tst);
            }
            finally
            {
                if (!tableInfo.BulkConfig.UseTempDB)
                    context.Database.ExecuteSqlCommand(SqlQueryBuilder.DropTable(tableInfo.FullTempTableName));

                if (keepIdentity && tableInfo.HasIdentityColumn)
                {
                    context.Database.ExecuteSqlCommand(SqlQueryBuilder.SetIdentityInsert(tableInfo.FullTableName, false));
                    context.Database.CloseConnection();
                }
            }
        }

        public static async Task MergeAsync<T>(DbContext context, IList<T> entities, TableInfo tableInfo) where T : class
        {
            tableInfo.InsertToTempTable = true;

            await context.Database.ExecuteSqlCommandAsync(SqlQueryBuilder.CreateTableCopy(tableInfo.FullTableName, tableInfo.FullTempTableName, tableInfo)).ConfigureAwait(false);

            bool keepIdentity = tableInfo.BulkConfig.SqlBulkCopyOptions.HasFlag(SqlBulkCopyOptions.KeepIdentity);
            try
            {
                await InsertAsync(context, entities, tableInfo).ConfigureAwait(false);

                if (keepIdentity && tableInfo.HasIdentityColumn)
                {
                    context.Database.OpenConnection();
                    context.Database.ExecuteSqlCommand(SqlQueryBuilder.SetIdentityInsert(tableInfo.FullTableName, true));
                }

                var tst = tableInfo.QueryOutputTable<T>(context, SqlQueryBuilder.MergeTable(tableInfo)).ToList();
                tableInfo.UpdateEntitiesIdentityIfNeeded<T>(entities, tst);
            }
            finally
            {
                if (!tableInfo.BulkConfig.UseTempDB)
                    await context.Database.ExecuteSqlCommandAsync(SqlQueryBuilder.DropTable(tableInfo.FullTempTableName)).ConfigureAwait(false);

                if (keepIdentity && tableInfo.HasIdentityColumn)
                {
                    context.Database.ExecuteSqlCommand(SqlQueryBuilder.SetIdentityInsert(tableInfo.FullTableName, false));
                    context.Database.CloseConnection();
                }
            }
        }
        #endregion

        #region DataTable
        internal static DataTable GetDataTable<T>(DbContext context, IList<T> entities, SqlBulkCopy sqlBulkCopy)
        {
            var dataTable = new DataTable();
            var columnsDict = new Dictionary<string, object>();
            var ownedEntitiesMappedProperties = new HashSet<string>();

            var type = typeof(T);
            var entityType = context.Model.FindEntityType(type);
            var entityPropertiesDict = entityType.GetProperties().ToDictionary(a => a.Name, a => a);
            var entityNavigationOwnedDict = entityType.GetNavigations().Where(a => a.GetTargetType().IsOwned()).ToDictionary(a => a.Name, a => a);
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                if (entityPropertiesDict.ContainsKey(property.Name))
                {
                    string columnName = entityPropertiesDict[property.Name].Relational().ColumnName;
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    dataTable.Columns.Add(columnName, propertyType);
                    columnsDict.Add(property.Name, null);
                }
                else if (entityNavigationOwnedDict.ContainsKey(property.Name)) // isOWned
                {
                    Type navOwnedType = type.Assembly.GetType(property.PropertyType.FullName);

                    var ownedEntityType = context.Model.FindEntityType(property.PropertyType);
                    if (ownedEntityType == null)
                    {
                        ownedEntityType = context.Model.GetEntityTypes().SingleOrDefault(a => a.DefiningNavigationName == property.Name && a.DefiningEntityType.Name == entityType.Name);
                    }
                    var ownedEntityProperties = ownedEntityType.GetProperties().ToList();
                    var ownedEntityPropertyNameColumnNameDict = new Dictionary<string, string>();

                    foreach (var ownedEntityProperty in ownedEntityProperties)
                    {
                        if (!ownedEntityProperty.IsPrimaryKey())
                        {
                            string columnName = ownedEntityProperty.Relational().ColumnName;
                            ownedEntityPropertyNameColumnNameDict.Add(ownedEntityProperty.Name, columnName);
                            ownedEntitiesMappedProperties.Add(property.Name + "_" + ownedEntityProperty.Name);
                        }
                    }

                    var innerProperties = property.PropertyType.GetProperties();
                    foreach (var innerProperty in innerProperties)
                    {
                        if (ownedEntityPropertyNameColumnNameDict.ContainsKey(innerProperty.Name))
                        {
                            string columnName = ownedEntityPropertyNameColumnNameDict[innerProperty.Name];
                            var ownedPropertyType = Nullable.GetUnderlyingType(innerProperty.PropertyType) ?? innerProperty.PropertyType;
                            dataTable.Columns.Add(columnName, ownedPropertyType);
                            columnsDict.Add(property.Name + "_" + innerProperty.Name, null);
                        }
                    }
                }
            }

            foreach (var entity in entities)
            {
                foreach (var property in properties)
                {
                    var propertyValue = property.GetValue(entity, null);
                    if (entityPropertiesDict.ContainsKey(property.Name))
                    {
                        columnsDict[property.Name] = propertyValue;
                    }
                    else if (entityNavigationOwnedDict.ContainsKey(property.Name))
                    {
                        var ownedProperties = property.PropertyType.GetProperties().Where(a => ownedEntitiesMappedProperties.Contains(property.Name + "_" + a.Name));
                        foreach (var ownedProperty in ownedProperties)
                        {
                            columnsDict[property.Name + "_" + ownedProperty.Name] = propertyValue == null ? null : ownedProperty.GetValue(propertyValue, null);
                        }
                    }
                }
                var record = columnsDict.Values.ToArray();
                dataTable.Rows.Add(record);
            }
            foreach (DataColumn item in dataTable.Columns)  //Add mapping
            {
                sqlBulkCopy.ColumnMappings.Add(item.ColumnName, item.ColumnName);
            }
            return dataTable;
        }
        #endregion

        #region Connection
        internal static SqlConnection OpenAndGetSqlConnection(DbContext context)
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return (SqlConnection)connection;
        }

        internal static async Task<SqlConnection> OpenAndGetSqlConnectionAsync(DbContext context)
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }
            return (SqlConnection)connection;
        }

        private static SqlBulkCopy GetSqlBulkCopy(SqlConnection sqlConnection, IDbContextTransaction transaction, BulkConfig config)
        {
            var sqlBulkCopyOptions = config.SqlBulkCopyOptions;
            if (transaction == null)
            {
                return new SqlBulkCopy(sqlConnection, sqlBulkCopyOptions, null);
            }
            else
            {
                var sqlTransaction = (SqlTransaction)transaction.GetDbTransaction();
                return new SqlBulkCopy(sqlConnection, sqlBulkCopyOptions, sqlTransaction);
            }
        }
        #endregion
    }
}
