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
using Paillave.Etl.EntityFrameworkCore.Core;
using Paillave.Etl.Reactive.Core;

namespace EFCore.BulkExtensions
{
    public class EntityMetadataExtractor<T> where T : class
    {
        private class PropertySet
        {
            public bool Insert { get; set; } // true for any column except computed, indentity, and timestamp
            public bool Update { get; set; } // true for any column except pivot, computed, indentity, and timestamp
            public bool Pivot { get; set; } // true for pivot column
            public bool GetAfterSetInTarget { get; set; } // true for computed, identity, timestamp, and with default value column
            public List<IProperty> Properties { get; set; }
        }
        private List<PropertySet> PropertySets { get; }
        public string TargetTableName { get; }
        public string StagingTableName { get; }
        public string OutputStagingTableName { get; }
        private IEnumerable<IEntityType> GetAllRelatedEntityTypes(IEntityType et)
        {
            yield return et;
            foreach (var item in et.GetDerivedTypes())
                foreach (var i in GetAllRelatedEntityTypes(item))
                    yield return i;
            // var allNavigationProperties = subClasses.SelectMany(et => et.GetNavigations()).Where(a => a.GetTargetType().IsOwned()).Distinct();
            // foreach (var navigationProperty in allNavigationProperties)
            // {
            //     var property = navigationProperty.PropertyInfo;
            //     var ownedEntityType = context.Model.FindEntityType(property.PropertyType);

            //     if (ownedEntityType == null) // when entity has more then one ownedType (e.g. Address HomeAddress, Address WorkAddress) or one ownedType is in multiple Entities like Audit is usually.
            //         ownedEntityType = context.Model.GetEntityTypes().SingleOrDefault(a => a.DefiningNavigationName == property.Name && a.DefiningEntityType.Name == entityType.Name);


            //     var ownedEntityPropertyNameColumnNameDict = ownedEntityType.GetProperties()
            //         .Where(ownedEntityProperty => !ownedEntityProperty.IsPrimaryKey())
            //         .ToDictionary(ownedEntityProperty => ownedEntityProperty.Name, ownedEntityProperty => ownedEntityProperty.Relational().ColumnName);

            //     foreach (var ownedProperty in property.PropertyType.GetProperties())
            //     {
            //         if (ownedEntityPropertyNameColumnNameDict.ContainsKey(ownedProperty.Name))
            //         {
            //             string columnName = ownedEntityPropertyNameColumnNameDict[ownedProperty.Name];
            //             var ownedPropertyType = Nullable.GetUnderlyingType(ownedProperty.PropertyType) ?? ownedProperty.PropertyType;
            //             PropertyColumnNamesDict.Add(property.Name + "." + ownedProperty.Name, columnName);
            //             OutputPropertyColumnNamesDict.Add(property.Name + "." + ownedProperty.Name, columnName);
            //         }
            //     }
            // }
        }
        public EntityMetadataExtractor(DbContext context, Expression<Func<T, object>> pivotKey)
        {
            List<IProperty> pivotProperties;
            IProperty identityProperty;
            List<IProperty> computedProperties;
            List<IProperty> defaultValuesProperties;

            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
                throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);
            TargetTableName = $"[{entityType.Relational().Schema ?? "dbo"}].[{entityType.Relational().TableName}]";
            var guidName = Guid.NewGuid().ToString().Substring(0, 8);
            StagingTableName = $"[{entityType.Relational().Schema ?? "dbo"}].[{entityType.Relational().TableName}_temp{guidName}]";
            OutputStagingTableName = $"[{entityType.Relational().Schema ?? "dbo"}].[{entityType.Relational().TableName}_tempoutput{guidName}]";

            var allRelatedEntityTypes = GetAllRelatedEntityTypes(entityType).Distinct();
            var allProperties = allRelatedEntityTypes.SelectMany(dt => dt.GetProperties()).Distinct(new LambdaEqualityComparer<IProperty, string>(i => i.Relational().ColumnName)).AsEnumerable();

            if (pivotKey != null)
            {
                pivotProperties = entityType.FindPrimaryKey().Properties.ToList();
            }
            else
            {
                var pivotKeyNames = KeyDefinitionExtractor.GetKeys(pivotKey).Select(i => i.Name).ToList();
                pivotProperties = allProperties.Where(i => pivotKeyNames.Contains(i.Name)).ToList();
            }

            identityProperty = entityType.GetProperties().FirstOrDefault(i => i.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn);
            // string timestampDbTypeName = nameof(TimestampAttribute).Replace("Attribute", "").ToLower(); // = "timestamp";
            computedProperties = allProperties.Where(i => i.ValueGenerated == ValueGenerated.OnAddOrUpdate).ToList();
            defaultValuesProperties = allProperties.Where(i => i.Relational().DefaultValueSql != null).ToList();
        }
    }
}
