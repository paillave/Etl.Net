using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Paillave.Etl.EntityFrameworkCore.Core;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.EntityFrameworkCore.BulkSave
{
    public abstract class BulkSaveEngineBase<T> where T : class
    {
        private List<IProperty> _propertiesToInsert; // any column except computed
        private List<IProperty> _propertiesToUpdate; // any column except pivot, computed
        private List<IProperty> _propertiesForPivot; // pivot columns
        private List<IProperty> _propertiesToGetAfterSetInTarget; // computed, and with default value column
        private List<IProperty> _propertiesToBulkLoad; // any column except computed that is not pivot
        private List<IEntityType> _entityTypes;

        private string _schema;
        private string _table;
        private readonly DbContext _context;
        protected abstract SaveContextQueryBase<T> CreateSaveContextQueryInstance(DbContext context, string schema, string table, List<IProperty> propertiesToInsert, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, List<IEntityType> entityTypes);
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
        public BulkSaveEngineBase(DbContext context, Expression<Func<T, object>> pivotKey = null)
        {
            List<IProperty> computedProperties;
            List<IProperty> defaultValuesProperties;
            List<IProperty> notPivotComputedProperties;
            this._context = context;

            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
                throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);
            _schema = entityType.Relational().Schema;
            _table = entityType.Relational().TableName;

            _entityTypes = GetAllRelatedEntityTypes(entityType).Distinct().ToList();
            var allProperties = _entityTypes.SelectMany(dt => dt.GetProperties()).Distinct(new LambdaEqualityComparer<IProperty, string>(i => i.Relational().ColumnName)).ToList();

            if (pivotKey == null)
            {
                _propertiesForPivot = entityType.FindPrimaryKey().Properties.ToList();
            }
            else
            {
                var pivotKeyNames = KeyDefinitionExtractor.GetKeys(pivotKey).Select(i => i.Name).ToList();
                _propertiesForPivot = allProperties.Where(i => pivotKeyNames.Contains(i.Name)).ToList();
            }

            //identityProperty = entityType.GetProperties().FirstOrDefault(i => i.SqlServer().ValueGenerationStrategy == SqlServerValueGenerationStrategy.IdentityColumn);
            // string timestampDbTypeName = nameof(TimestampAttribute).Replace("Attribute", "").ToLower(); // = "timestamp";
            computedProperties = allProperties.Where(i => (i.ValueGenerated & ValueGenerated.OnAddOrUpdate) != ValueGenerated.Never).ToList();
            notPivotComputedProperties = computedProperties.Except(_propertiesForPivot).ToList();

            defaultValuesProperties = allProperties.Where(i => i.Relational().DefaultValueSql != null).ToList();

            _propertiesToBulkLoad = allProperties.Except(notPivotComputedProperties).ToList();

            _propertiesToInsert = allProperties
                .Except(computedProperties)
                //.Except(new[] { identityProperty })
                .ToList();
            _propertiesToUpdate = allProperties
                .Except(_propertiesForPivot)
                .Except(computedProperties)
                //.Except(new[] { identityProperty })
                .ToList();
            _propertiesToGetAfterSetInTarget = computedProperties
                //.Union(new[] { identityProperty })
                .Union(defaultValuesProperties)
                .Distinct(new LambdaEqualityComparer<IProperty, string>(i => i.Relational().ColumnName))
                .ToList();
        }
        // public void Update<TSource, TKey>(IList<TSource> entities, Expression<Func<TSource, T>> updateKey, Expression<Func<TSource, T>> updateValues)
        // {
        //     var previousAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
        //     _context.ChangeTracker.AutoDetectChangesEnabled = false;
        //     var contextQuery = this.CreateContextQueryInstance(_context, _schema, _table, _propertiesToInsert, _propertiesToUpdate, _propertiesForPivot, _propertiesToBulkLoad, _entityTypes);

        //     _context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
        // }
        public void Save(IList<T> entities)
        {
            var previousAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            var contextQuery = this.CreateSaveContextQueryInstance(_context, _schema, _table, _propertiesToInsert, _propertiesToUpdate, _propertiesForPivot, _propertiesToBulkLoad, _entityTypes);
            SetDiscriminatorValue(entities);
            contextQuery.CreateStagingTable();
            contextQuery.CreateOutputStagingTable();
            contextQuery.BulkSaveInStaging(entities);
            if (entities.Count > 10000)
                contextQuery.IndexStagingTable();
            contextQuery.MergeFromStaging();
            if (entities.Count > 10000)
                contextQuery.IndexOutputStagingTable();
            IList<T> resultEntities;
            if (_propertiesToUpdate.Count > 0)
                resultEntities = contextQuery.GetOutputStaging();
            else
                resultEntities = contextQuery.GetOutputStagingForComputedColumns();
            UpdateInputEntities(_propertiesToGetAfterSetInTarget, entities, resultEntities);
            contextQuery.DeleteStagingTable();
            contextQuery.DeleteOutputStagingTable();
            _context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
        }
        private void SetDiscriminatorValue(IList<T> entities)
        {
            if (_entityTypes.All(i => i.Relational().DiscriminatorProperty == null)) return;
            var discriminatorsDictionary = _entityTypes
                .Select(i => new
                {
                    i.ClrType,
                    EntityType = i,
                    RelationalProperty = i.Relational()
                })
                .Where(i => i.RelationalProperty.DiscriminatorProperty != null)
                .ToDictionary(
                    i => i.ClrType,
                    i => new Dictionary<string, object>(new[] { new KeyValuePair<string, object>(i.RelationalProperty.DiscriminatorProperty.Name, i.RelationalProperty.DiscriminatorValue) }));
            foreach (var entity in entities)
            {
                var type = entity.GetType();
                var relationalProperty = discriminatorsDictionary[type];
                this._context.Entry(entity).CurrentValues.SetValues(relationalProperty);
            }
        }
        private void UpdateInputEntities(List<IProperty> propertiesToGetAfterSetInTarget, IList<T> inputEntities, IList<T> resultEntities)
        {
            for (int i = 0; i < inputEntities.Count; i++)
            {
                var inputEntity = inputEntities[i];
                var resultEntity = resultEntities[i];
                var dicoToSet = propertiesToGetAfterSetInTarget.ToDictionary(p => p.Name, p => p.PropertyInfo.GetValue(resultEntity));
                var entry = _context.Entry(inputEntity);
                entry.CurrentValues.SetValues(dicoToSet);
                entry.OriginalValues.SetValues(dicoToSet);
                foreach (var item in propertiesToGetAfterSetInTarget.Where(j => !j.IsShadowProperty).Select(j => j.PropertyInfo).ToList())
                    item.SetValue(inputEntity, item.GetValue(resultEntity));
                //_context.Entry(inputEntity).State = EntityState.Unchanged;
            }
        }
    }
}
