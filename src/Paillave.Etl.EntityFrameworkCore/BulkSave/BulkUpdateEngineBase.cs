﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Paillave.Etl.EntityFrameworkCore.Core;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.EntityFrameworkCore.BulkSave
{
    public abstract class BulkUpdateEngineBase<T, TSource> where T : class
    {
        private List<IProperty> _propertiesToUpdate; // any column except pivot, computed
        private List<IProperty> _propertiesForPivot; // pivot columns
        private List<IProperty> _propertiesToBulkLoad; // any column except computed that is not pivot
        private IDictionary<string, MemberInfo> _propertyGetters;
        private IEntityType _baseType;

        private string _schema;
        private string _table;
        private readonly DbContext _context;
        protected abstract UpdateContextQueryBase<TSource> CreateUpdateContextQueryInstance(DbContext context, string schema, string table, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, IEntityType baseType, IDictionary<string, MemberInfo> propertiesGetter);
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
        public BulkUpdateEngineBase(DbContext context, Expression<Func<TSource, T>> updateKey, Expression<Func<TSource, T>> updateValues)
        {
            List<IProperty> computedProperties;
            this._context = context;

            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
                throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);
            _schema = entityType.Relational().Schema;
            _table = entityType.Relational().TableName;

            List<IEntityType> entityTypes = GetAllRelatedEntityTypes(entityType).Distinct().ToList();
            _baseType = entityTypes.FirstOrDefault(i => i.BaseType == null);
            var allProperties = entityTypes.SelectMany(dt => dt.GetProperties()).Distinct(new LambdaEqualityComparer<IProperty, string>(i => i.Relational().ColumnName)).ToList();
            computedProperties = allProperties.Where(i => (i.ValueGenerated & ValueGenerated.OnAddOrUpdate) != ValueGenerated.Never).ToList();

            var valuesSetters = SettersExtractor.GetGetters(updateValues);
            var keySetters = SettersExtractor.GetGetters(updateKey);

            _propertyGetters = valuesSetters.Union(keySetters).ToDictionary(i => i.Key, i => i.Value);

            _propertiesForPivot = allProperties.Where(i => keySetters.ContainsKey(i.Name)).ToList();
            _propertiesToUpdate = allProperties.Where(i => valuesSetters.ContainsKey(i.Name)).Except(computedProperties).Except(_propertiesForPivot).ToList();
            _propertiesToBulkLoad = _propertiesForPivot.Union(_propertiesToUpdate).ToList();
        }
        public void Update(IList<TSource> sources)
        {
            var previousAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            var contextQuery = this.CreateUpdateContextQueryInstance(_context, _schema, _table, _propertiesToUpdate, _propertiesForPivot, _propertiesToBulkLoad, _baseType, _propertyGetters);
            contextQuery.CreateStagingTable();
            contextQuery.BulkSaveInStaging(sources);
            if (sources.Count > 10000)
                contextQuery.IndexStagingTable();
            contextQuery.MergeFromStaging();
            contextQuery.DeleteStagingTable();
            _context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
        }
    }
}
