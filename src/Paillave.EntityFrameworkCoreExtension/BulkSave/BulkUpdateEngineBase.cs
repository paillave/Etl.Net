using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Paillave.EntityFrameworkCoreExtension.ContextMetadata;
using Paillave.EntityFrameworkCoreExtension.Core;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave;

public abstract class BulkUpdateEngineBase<T, TSource> where T : class
{
    private List<IProperty> _propertiesToUpdate; // any column except pivot, computed
    private List<IProperty> _propertiesForPivot; // pivot columns
    private List<IProperty> _propertiesToBulkLoad; // any column except computed that is not pivot
    private IDictionary<string, MemberInfo> _propertyGetters;
    private IEntityType _baseType;
    protected StoreObjectIdentifier StoreObject { get; }

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
    }
    public BulkUpdateEngineBase(DbContext context, Expression<Func<TSource, T>> updateKey, Expression<Func<TSource, T>> updateValues)
    {
        List<IProperty> computedProperties;
        this._context = context;
        var entityType = context.Model.FindEntityType(typeof(T));
        if (entityType == null)
            throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);
        this.StoreObject = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table).GetValueOrDefault();
        var summary = entityType.GetEntityEssentials();
        _schema = summary.Schema;
        var table = entityType.GetTableName();
        if (string.IsNullOrWhiteSpace(table))
            throw new InvalidOperationException("Table name is not defined for Type: " + typeof(T).Name);
        _table = table;
        List<IEntityType> entityTypes = GetAllRelatedEntityTypes(entityType).Distinct().ToList();
        _baseType = entityTypes.First(i => i.BaseType == null);
        var allProperties = entityTypes.SelectMany(dt => dt.GetProperties()).DistinctBy(i => i.GetColumnName(this.StoreObject)).ToList();
        computedProperties = allProperties.Where(i => (i.ValueGenerated & ValueGenerated.OnAddOrUpdate) != ValueGenerated.Never).ToList();

        var valuesSetters = SettersExtractor.GetGetters(updateValues);
        var keySetters = SettersExtractor.GetGetters(updateKey);
        if (typeof(IMultiTenantEntity).IsAssignableFrom(typeof(T)))
        {
            var tenantIdProp = allProperties.First(i => i.Name == nameof(IMultiTenantEntity.TenantId));
            var columnName = tenantIdProp.GetColumnName(this.StoreObject);
            if (columnName == null)
                throw new InvalidOperationException("TenantId column is not defined for Type: " + typeof(T).Name);
            if (!keySetters.ContainsKey(columnName))
            {
                if (tenantIdProp.PropertyInfo == null)
                    throw new InvalidOperationException("TenantId property is not defined for Type: " + typeof(T).Name);
                keySetters[columnName] = tenantIdProp.PropertyInfo;
            }
        }
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
