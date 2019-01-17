using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using FastMember;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Paillave.Etl.Reactive.Core;

namespace EFCore.BulkExtensions
{
    public class TableInfo
    {
        public string Schema { get; set; }
        public string SchemaFormated => Schema != null ? $"[{Schema}]." : "";
        public string TableName { get; set; }
        public string FullTableName => $"{SchemaFormated}[{TableName}]";
        public List<string> PivotKeys { get; set; }
        public bool UpdateByPropertiesAreNullable { get; set; }
        protected string TempDBPrefix => BulkConfig.UseTempDB ? "#" : "";
        public string TempTableSuffix { get; set; }
        public string TempTableName => $"{TableName}{TempTableSuffix}";
        public string FullTempTableName => $"{SchemaFormated}[{TempDBPrefix}{TempTableName}]";
        public string FullTempOutputTableName => $"{SchemaFormated}[{TempDBPrefix}{TempTableName}Output]";
        public string IdentityColumn = null;
        public bool HasIdentityColumn => !string.IsNullOrWhiteSpace(IdentityColumn);
        public bool InsertToTempTable { get; set; }
        public int NumberOfEntities { get; set; }
        public BulkConfig BulkConfig { get; set; }
        public Dictionary<string, string> OutputPropertyColumnNamesDict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> PropertyColumnNamesDict { get; set; } = new Dictionary<string, string>();
        public HashSet<string> ShadowProperties { get; set; } = new HashSet<string>();
        public Dictionary<string, ValueConverter> ConvertibleProperties { get; set; } = new Dictionary<string, ValueConverter>();
        public string TimeStampColumnName { get; set; }
        public static TableInfo CreateInstance<T>(DbContext context, IList<T> entities, BulkConfig bulkConfig)
        {
            var tableInfo = new TableInfo
            {
                NumberOfEntities = entities.Count,
                BulkConfig = bulkConfig ?? new BulkConfig()
            };

            bool isExplicitTransaction = context.Database.GetDbConnection().State == ConnectionState.Open;
            if (tableInfo.BulkConfig.UseTempDB && !isExplicitTransaction)
            {
                throw new InvalidOperationException("UseTempDB when set then BulkOperation has to be inside Transaction. More info in README of the library in GitHub.");
                // Otherwise throws exception: 'Cannot access destination table' (gets Dropped too early because transaction ends before operation is finished)
            }

            tableInfo.LoadData<T>(context);
            return tableInfo;
        }
        #region Main
        public void LoadData<T>(DbContext context)
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
                throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);

            var relationalData = entityType.Relational();
            Schema = relationalData.Schema ?? "dbo";
            TableName = relationalData.TableName;
            TempTableSuffix = "Temp" + Guid.NewGuid().ToString().Substring(0, 8); // 8 chars of Guid as tableNameSuffix to avoid same name collision with other tables

            bool AreSpecifiedUpdateByProperties = BulkConfig.UpdateByProperties?.Count() > 0;
            var primaryKeys = entityType.FindPrimaryKey().Properties.Select(a => a.Name).ToList();
            PivotKeys = AreSpecifiedUpdateByProperties ? BulkConfig.UpdateByProperties : primaryKeys;

            IdentityColumn = entityType.GetProperties().FirstOrDefault(i => i.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn)?.Name;

            var allProperties = entityType.GetProperties().AsEnumerable();


            //var subClassProperties = entityType.GetDerivedTypes().SelectMany(dt => dt.GetProperties()).Distinct(new LambdaEqualityComparer<IProperty, string>(i => i.Relational().ColumnName));


            var allNavigationProperties = entityType.GetNavigations().Where(a => a.GetTargetType().IsOwned());

            // timestamp/row version properties are only set by the Db, the property has a [Timestamp] Attribute or is configured in in FluentAPI with .IsRowVersion()
            // They can be identified by the column type "timestamp" or .IsConcurrencyToken in combination with .ValueGenerated == ValueGenerated.OnAddOrUpdate
            string timestampDbTypeName = nameof(TimestampAttribute).Replace("Attribute", "").ToLower(); // = "timestamp";
            var timeStampProperties = allProperties.Where(a => (a.IsConcurrencyToken && a.ValueGenerated == ValueGenerated.OnAddOrUpdate) || a.Relational().ColumnType == timestampDbTypeName);
            TimeStampColumnName = timeStampProperties.FirstOrDefault()?.Relational().ColumnName; // can be only One
            var allPropertiesExceptTimeStamp = allProperties.Except(timeStampProperties);
            var properties = allPropertiesExceptTimeStamp.Where(a => a.Relational().ComputedColumnSql == null);

            // TimeStamp prop. is last column in OutputTable since it is added later with varbinary(8) type in which Output can be inserted
            OutputPropertyColumnNamesDict = allPropertiesExceptTimeStamp.Concat(timeStampProperties).ToDictionary(a => a.Name, b => b.Relational().ColumnName);

            UpdateByPropertiesAreNullable = properties.Any(a => PivotKeys.Contains(a.Name) && a.IsNullable);

            PropertyColumnNamesDict = properties.ToDictionary(a => a.Name, b => b.Relational().ColumnName);
            ShadowProperties = new HashSet<string>(properties.Where(p => p.IsShadowProperty).Select(p => p.Relational().ColumnName));




            ConvertibleProperties = properties
                .Where(p => p.GetValueConverter() != null)
                .ToDictionary(property => property.Relational().ColumnName, property => property.GetValueConverter());

            foreach (var navigationProperty in allNavigationProperties)
            {
                var property = navigationProperty.PropertyInfo;
                var ownedEntityType = context.Model.FindEntityType(property.PropertyType);

                if (ownedEntityType == null) // when entity has more then one ownedType (e.g. Address HomeAddress, Address WorkAddress) or one ownedType is in multiple Entities like Audit is usually.
                    ownedEntityType = context.Model.GetEntityTypes().SingleOrDefault(a => a.DefiningNavigationName == property.Name && a.DefiningEntityType.Name == entityType.Name);


                var ownedEntityPropertyNameColumnNameDict = ownedEntityType.GetProperties()
                    .Where(ownedEntityProperty => !ownedEntityProperty.IsPrimaryKey())
                    .ToDictionary(ownedEntityProperty => ownedEntityProperty.Name, ownedEntityProperty => ownedEntityProperty.Relational().ColumnName);

                foreach (var ownedProperty in property.PropertyType.GetProperties())
                {
                    if (ownedEntityPropertyNameColumnNameDict.ContainsKey(ownedProperty.Name))
                    {
                        string columnName = ownedEntityPropertyNameColumnNameDict[ownedProperty.Name];
                        var ownedPropertyType = Nullable.GetUnderlyingType(ownedProperty.PropertyType) ?? ownedProperty.PropertyType;
                        PropertyColumnNamesDict.Add(property.Name + "." + ownedProperty.Name, columnName);
                        OutputPropertyColumnNamesDict.Add(property.Name + "." + ownedProperty.Name, columnName);
                    }
                }
            }
        }
        public void SetSqlBulkCopyConfig<T>(SqlBulkCopy sqlBulkCopy, IList<T> entities)
        {
            sqlBulkCopy.DestinationTableName = InsertToTempTable ? FullTempTableName : FullTableName;
            sqlBulkCopy.BatchSize = BulkConfig.BatchSize;
            sqlBulkCopy.NotifyAfter = BulkConfig.NotifyAfter ?? BulkConfig.BatchSize;
            sqlBulkCopy.SqlRowsCopied += (sender, e) =>
            {
                // progress?.Invoke((decimal)(e.RowsCopied * 10000 / entities.Count) / 10000); // round to 4 decimal places
            };
            sqlBulkCopy.BulkCopyTimeout = BulkConfig.BulkCopyTimeout ?? sqlBulkCopy.BulkCopyTimeout;
            sqlBulkCopy.EnableStreaming = BulkConfig.EnableStreaming;
        }
        #endregion
        public void UpdateEntitiesIdentityIfNeeded<T>(IList<T> entities, IList<T> entitiesWithOutputIdentity)
        {
            if (BulkConfig.SetOutputIdentity && HasIdentityColumn)
            {
                var accessor = TypeAccessor.Create(typeof(T), true);
                for (int i = 0; i < NumberOfEntities; i++)
                    accessor[entities[i], IdentityColumn] = accessor[entitiesWithOutputIdentity[i], IdentityColumn];
            }
        }
        #region CompiledQuery
        public IEnumerable<T> QueryOutputTable<T>(DbContext context, string sqlQuery) where T : class
        {
            var compiled = EF.CompileQuery(GetQueryExpression<T>(sqlQuery));
            var result = compiled(context);
            return result;
        }
        public Expression<Func<DbContext, IQueryable<T>>> GetQueryExpression<T>(string sqlQuery) where T : class
        {
            Expression<Func<DbContext, IQueryable<T>>> expression = null;
            if (BulkConfig.TrackingEntities) // If Else can not be replaced with Ternary operator for Expression
                expression = (ctx) => ctx.Set<T>().IgnoreQueryFilters().FromSql(sqlQuery);
            else
                expression = (ctx) => ctx.Set<T>().IgnoreQueryFilters().FromSql(sqlQuery).AsNoTracking();
            return OrderBy(expression, PivotKeys[0]);
        }
        private static Expression<Func<DbContext, IQueryable<T>>> OrderBy<T>(Expression<Func<DbContext, IQueryable<T>>> source, string ordering)
        {
            Type entityType = typeof(T);
            PropertyInfo property = entityType.GetProperty(ordering);
            ParameterExpression parameter = Expression.Parameter(entityType);
            MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);
            LambdaExpression orderByExp = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderBy", new Type[] { entityType, property.PropertyType }, source.Body, Expression.Quote(orderByExp));
            return Expression.Lambda<Func<DbContext, IQueryable<T>>>(resultExp, source.Parameters);
        }
        #endregion
    }
}
