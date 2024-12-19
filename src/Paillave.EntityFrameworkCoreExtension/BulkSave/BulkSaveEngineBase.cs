using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Paillave.EntityFrameworkCoreExtension.ContextMetadata;
using Paillave.EntityFrameworkCoreExtension.Core;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave;

public abstract class BulkSaveEngineBase<T> : IDisposable where T : class
{
    private List<IProperty> _propertiesToInsert; // any column except computed
    private List<IProperty> _propertiesToUpdate; // any column except pivot, computed
                                                 // private HashSet<string> _propertiesNotToBeUpdatedToNull = new HashSet<string>();
    private List<List<IProperty>> _propertiesForPivotSet; // pivot columns
                                                          // private List<IProperty> _propertiesForPivot; // pivot columns
    private List<IProperty> _propertiesToGetAfterSetInTarget; // computed, and with default value column
    private List<IProperty> _propertiesToBulkLoad; // any column except computed that is not pivot
    private List<IEntityType> _entityTypes;

    protected StoreObjectIdentifier StoreObject { get; }
    private string? _schema;
    private string _table;
    private bool disposedValue;
    private readonly DbContext _context;
    protected abstract SaveContextQueryBase<T> CreateSaveContextQueryInstance(DbContext context, string? schema, string table, List<IProperty> propertiesToInsert, List<IProperty> propertiesToUpdate, List<List<IProperty>> propertiesForPivotSet, List<IProperty> propertiesToBulkLoad, List<IEntityType> entityTypes, CancellationToken cancellationToken);
    private IEnumerable<IEntityType> GetAllRelatedEntityTypes(IEntityType et)
    {
        yield return et;
        foreach (var item in et.GetDerivedTypes())
            foreach (var i in GetAllRelatedEntityTypes(item))
                yield return i;
    }
    private List<IProperty> GetPropertiesForPivot(List<IProperty> allProperties, IProperty? tenantIdProp, Expression<Func<T, object>> pivotKey)
    {
        var pivotKeyNames = KeyDefinitionExtractor.GetKeys(pivotKey).Select(i => i.Name).ToList();
        if (tenantIdProp != null)
        {
            var columnName = tenantIdProp.GetColumnName(StoreObject);
            if (string.IsNullOrWhiteSpace(columnName))
                throw new InvalidOperationException($"The property {tenantIdProp.Name} does not have a column name defined");
            pivotKeyNames.Add(columnName);
        }
        return allProperties.Where(i => pivotKeyNames.Contains(i.Name)).ToList();
    }
    public BulkSaveEngineBase(DbContext context, params Expression<Func<T, object>>[]? pivotKeys)
    {
        List<IProperty> dbComputedProperties;
        List<IProperty> defaultValuesProperties;
        List<IProperty> notPivotComputedProperties;
        this._context = context;

        var entityType = context.Model.FindEntityType(typeof(T));
        if (entityType == null)
            throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);
        StoreObject = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table).GetValueOrDefault();
        var summary = entityType.GetEntityEssentials();
        _schema = summary.Schema;
        var table = entityType.GetTableName();
        if (string.IsNullOrWhiteSpace(table))
            throw new InvalidOperationException("Table name is not defined");
        _table = table;

        _entityTypes = GetAllRelatedEntityTypes(entityType).Distinct().ToList();
        var allProperties = _entityTypes
            .SelectMany(dt => dt.GetProperties())
            .DistinctBy(i => i.GetColumnName(StoreObject))
            .ToList();

        if (pivotKeys == null || pivotKeys.Length == 0)
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey != null)
                _propertiesForPivotSet = new[] { primaryKey.Properties.ToList() }.ToList();
            else
                _propertiesForPivotSet = new List<List<IProperty>>();
        }
        else
        {
            IProperty? tenantIdProp = null;

            if (typeof(IMultiTenantEntity).IsAssignableFrom(typeof(T)))
            {
                tenantIdProp = allProperties.First(i => i.Name == nameof(IMultiTenantEntity.TenantId));
            }

            _propertiesForPivotSet = pivotKeys.Select(pivotKey => this.GetPropertiesForPivot(allProperties, tenantIdProp, pivotKey)).ToList();
        }

        dbComputedProperties = allProperties.Where(i => (i.ValueGenerated & ValueGenerated.OnAddOrUpdate) != ValueGenerated.Never).ToList();
        notPivotComputedProperties = dbComputedProperties.Except(_propertiesForPivotSet.SelectMany(i => i)).ToList();

        defaultValuesProperties = allProperties.Where(i => i.GetDefaultValueSql() != null).ToList();

        _propertiesToBulkLoad = allProperties.Except(notPivotComputedProperties).ToList();

        _propertiesToInsert = allProperties
            .Except(dbComputedProperties)
            .ToList();
        _propertiesToUpdate = allProperties
            .Except(dbComputedProperties)
            .ToList();
        _propertiesToGetAfterSetInTarget = dbComputedProperties
            .Union(defaultValuesProperties)
            .DistinctBy(i => i.GetColumnName(StoreObject))
            .ToList();
    }
    public void Save(IList<T> entities, CancellationToken cancellationToken, bool doNotUpdateIfExists = false, bool insertOnly = false)
    {
        if (entities.Count == 0) return;
        var previousAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
        _context.ChangeTracker.AutoDetectChangesEnabled = false;
        this.TrySetTenant(entities);
        var contextQuery = this.CreateSaveContextQueryInstance(_context, _schema, _table, _propertiesToInsert, _propertiesToUpdate, _propertiesForPivotSet, _propertiesToBulkLoad, _entityTypes, cancellationToken);
        SetDiscriminatorValue(entities);
        bool outputStagingTableCreated = false;
        contextQuery.CreateStagingTable();
        try
        {
            contextQuery.BulkSaveInStaging(entities);
            if (!insertOnly && this.ShouldIndexStagingTable(entities.Count()))
                foreach (var propertiesForPivot in this._propertiesForPivotSet)
                    contextQuery.IndexStagingTable(propertiesForPivot);

            contextQuery.CreateOutputStagingTable();
            outputStagingTableCreated = true;
            if (insertOnly)
                contextQuery.InsertFromStaging();
            else
                contextQuery.MergeFromStaging(doNotUpdateIfExists);
            if (this.ShouldIndexStagingTable(entities.Count()))
                contextQuery.IndexOutputStagingTable();

            DataTable resultEntities;
            if (_propertiesToUpdate.Count > 0)
                resultEntities = contextQuery.GetOutputStaging();
            else
                resultEntities = contextQuery.GetOutputStagingForComputedColumns();
            UpdateInputEntities(_propertiesToGetAfterSetInTarget, entities, resultEntities);

            _context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
        }
        finally
        {
            contextQuery.DeleteStagingTable();
            if (outputStagingTableCreated)
                contextQuery.DeleteOutputStagingTable();
        }
    }
    protected virtual bool ShouldIndexStagingTable(int entitiesCount) => entitiesCount >= 10000;
    private void TrySetTenant(IList<T> entities)
    {
        if (this._context is MultiTenantDbContext mtCtx)
        {
            mtCtx.UpdateEntitiesForMultiTenancy(entities);
        }
    }

    private void SetDiscriminatorValue(IList<T> entities)
    {
        if (_entityTypes.All(i => i.FindDiscriminatorProperty() == null)) return;
        var discriminatorsDictionary = _entityTypes
            .Select(i => new
            {
                i.ClrType,
                EntityType = i,
                DiscriminatorProperty = i.FindDiscriminatorProperty(),
                DiscriminatorValue = i.FindDiscriminatorProperty() != null ? i.GetDiscriminatorValue() : null
                // RelationalProperty = i.Relational()
            })
            .Where(i => i.DiscriminatorProperty != null)
            .ToDictionary(
                i => i.ClrType,
                i => new Dictionary<string, object>(new[] { new KeyValuePair<string, object>(i.DiscriminatorProperty!.Name, i.DiscriminatorValue ?? throw new Exception("no DiscriminatorValue")) }));
        foreach (var entity in entities)
        {
            var type = entity.GetType();
            var relationalProperty = discriminatorsDictionary[type];
            this._context.Entry(entity).CurrentValues.SetValues(relationalProperty);
        }
    }
    private void UpdateInputEntities(List<IProperty> propertiesToGetAfterSetInTarget, IList<T> inputEntities, DataTable resultEntities)
    {
        var resultColumns = resultEntities.Columns.OfType<DataColumn>().ToDictionary(i => i.ColumnName.ToLower());
        for (int i = 0; i < inputEntities.Count; i++)
        {
            var inputEntity = inputEntities[i];
            var resultDataRow = resultEntities.Rows[i];
            var entry = _context.Entry(inputEntity);
            var dicoToSet = propertiesToGetAfterSetInTarget
                .Where(j => !j.IsPrimaryKey() || string.Equals(GetDataReaderAction(resultDataRow, resultColumns), "INSERT", StringComparison.InvariantCultureIgnoreCase))
                .Where(j => j.DeclaringType.ClrType.IsAssignableFrom(entry.Metadata.ClrType))
                .Select(p => new { p.Name, Value = this.GetDataReaderValue(resultDataRow, resultColumns, p) })
                .Where(p => p.Value != Constants.DBNull)
                .ToDictionary(p => p.Name, p => p.Value);
            entry.CurrentValues.SetValues(dicoToSet); // TODO: do not update key if already exists
            entry.OriginalValues.SetValues(dicoToSet);
            foreach (var item in propertiesToGetAfterSetInTarget.Where(j => !j.IsShadowProperty() && j.DeclaringType.ClrType.IsAssignableFrom(entry.Metadata.ClrType)).ToList())
            {
                var value = this.GetDataReaderValue(resultDataRow, resultColumns, item);
                if (item.PropertyInfo == null)
                    throw new InvalidOperationException($"The property {item.Name} does not have a property info defined");
                if (value != Constants.DBNull) item.PropertyInfo.SetValue(inputEntity, value);
            }
        }
    }
    private string? GetDataReaderAction(DataRow dataRow, Dictionary<string, DataColumn> dataColumns)
    {
        var columnName = "_Action";
        var dataColumn = dataColumns[columnName.ToLower()];
        return dataRow.ItemArray[dataColumn.Ordinal] as string;
    }
    private object? GetDataReaderValue(DataRow dataRow, Dictionary<string, DataColumn> dataColumns, IProperty property)
    {
        var columnName = property.GetColumnName(StoreObject);
        if (string.IsNullOrWhiteSpace(columnName))
            throw new InvalidOperationException($"The property {property.Name} does not have a column name defined");
        var dataColumn = dataColumns[columnName.ToLower()];
        var dataRowValue = dataRow.ItemArray[dataColumn.Ordinal];
        var converter = property.GetTypeMapping().Converter;
        if (converter == null)
        {
            return dataRowValue;
        }
        return converter.ConvertFromProvider(dataRowValue);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
