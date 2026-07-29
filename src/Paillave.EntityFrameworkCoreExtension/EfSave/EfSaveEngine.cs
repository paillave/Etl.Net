using Microsoft.EntityFrameworkCore;
using Paillave.EntityFrameworkCoreExtension.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.EntityFrameworkCoreExtension.EfSave;

public class EfSaveEngine<T> where T : class
{
    private readonly Expression<Func<T, T, bool>> _findConditionExpression;
    private readonly List<PropertyInfo> _keyPropertyInfos;
    private readonly DbContext _context;
    private readonly CancellationToken _cancellationToken;
    public EfSaveEngine(DbContext context, CancellationToken cancellationToken, params Expression<Func<T, object>>[] pivotKeys)
    {
        this._cancellationToken = cancellationToken;
        _context = context;
        var entityType = context.Model.FindEntityType(typeof(T)) ?? throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);
        _keyPropertyInfos = entityType.GetProperties()
            .Where(i => !i.IsShadowProperty() && i.IsPrimaryKey())
            .Where(i => i.PropertyInfo != null)
            .Select(i => i.PropertyInfo!)
            .ToList();
        List<List<PropertyInfo>> propertyInfosForPivot;
        if ((pivotKeys?.Length ?? 0) == 0)
        {
            var pk = entityType.FindPrimaryKey();
            if (pk == null)
                propertyInfosForPivot = new List<List<PropertyInfo>>();
            else
                propertyInfosForPivot = new List<List<PropertyInfo>> { pk.Properties
                .Where(i => i.PropertyInfo != null)
                .Select(i => i.PropertyInfo!)
                .ToList() };
        }
        else
        {
            if (pivotKeys != null)
            {
                propertyInfosForPivot = pivotKeys.Select(pivotKey => KeyDefinitionExtractor.GetKeys(pivotKey))
                    .ToList();
            }
            else
            {
                propertyInfosForPivot = new List<List<PropertyInfo>>();
            }
        }

        _findConditionExpression = CreateFindConditionExpression(propertyInfosForPivot);
    }
    public EfSaveEngine(DbContext context, CancellationToken cancellationToken, Expression<Func<T, T, bool>> pivotCondition)
    {
        this._cancellationToken = cancellationToken;
        _context = context;
        var entityType = context.Model.FindEntityType(typeof(T)) ?? throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);
        _keyPropertyInfos = entityType.GetProperties()
            .Where(i => !i.IsShadowProperty() && i.IsPrimaryKey())
            .Where(i => i.PropertyInfo != null)
            .Select(i => i.PropertyInfo!)
            .ToList();
        _findConditionExpression = pivotCondition;
    }
    private Expression<Func<T, T, bool>> CreateFindConditionExpression(List<List<PropertyInfo>> propertyInfosForPivotSet)
    {
        ParameterExpression leftParam = Expression.Parameter(typeof(T), "i");
        ParameterExpression rightParam = Expression.Parameter(typeof(T), "rightParam");
        Expression? predicateBody = null;
        foreach (var propertyInfosForPivot in propertyInfosForPivotSet)
        {
            var pivotPartExpression = CreatePivotPartExpression(propertyInfosForPivot, leftParam, rightParam);
            if (predicateBody == null)
                predicateBody = pivotPartExpression;
            else
                predicateBody = Expression.OrElse(predicateBody, pivotPartExpression);
        }
        if(predicateBody == null)
            throw new InvalidOperationException("No pivot key found");
        return Expression.Lambda<Func<T, T, bool>>(predicateBody, new[] { leftParam, rightParam });
    }
    private Expression CreatePivotPartExpression(List<PropertyInfo> propertyInfosForPivot, ParameterExpression leftParam, ParameterExpression rightParam)
    {
        Expression? predicatePivotPart = null;
        foreach (var propertyInfoForPivot in propertyInfosForPivot)
        {
            var equalityExpression = CreateEqualityExpression(propertyInfoForPivot, leftParam, rightParam);
            if (predicatePivotPart == null)
                predicatePivotPart = equalityExpression;
            else
                predicatePivotPart = Expression.AndAlso(predicatePivotPart, equalityExpression);
        }
        if(predicatePivotPart == null)
            throw new InvalidOperationException("No pivot key found");
        return predicatePivotPart;
    }
    private Expression CreateEqualityExpression(PropertyInfo propertyInfo, ParameterExpression leftParam, ParameterExpression rightParam)
    {
        Expression leftValue = Expression.Property(leftParam, propertyInfo);
        Expression rightValue = Expression.Property(rightParam, propertyInfo);
        Expression equality = Expression.Equal(leftValue, rightValue);
        if (IsPotentiallyNull(propertyInfo.PropertyType))
        {
            // matches SqlServerSaveContextQuery.CreateEqualityConditionSql: a null seek value on the incoming
            // entity must not match rows where the target column is also null, otherwise this pivot key would
            // never fall through to the next AlternativelySeekOn key when the value is missing.
            Expression leftIsNotNull = Expression.NotEqual(leftValue, Expression.Constant(null, propertyInfo.PropertyType));
            return Expression.AndAlso(leftIsNotNull, equality);
        }
        return equality;
    }
    private static bool IsPotentiallyNull(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    public async Task SaveAsync(IList<T> entities, bool doNotUpdateIfExists = false, bool insertOnly = false)
    {
        var contextSet = _context.Set<T>();
        foreach (var entity in entities)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }
            if (insertOnly)
                contextSet.Add(entity);
            else
                InsertOrUpdateEntity(doNotUpdateIfExists, contextSet, entity);
        }
        await _context.SaveChangesAsync(_cancellationToken);
    }

    private void InsertOrUpdateEntity(bool doNotUpdateIfExists, DbSet<T> contextSet, T entity)
    {
        var entityCondition = _findConditionExpression.ApplyPartialLeft(entity);
        var existingEntity = contextSet.AsNoTracking().FirstOrDefault(entityCondition);
        if (existingEntity == null)
        {
            // For an entity with a store-generated surrogate Id, contextSet.Update(entity) here happened to
            // be harmless: EF Core auto-promotes an Update() call on an entity whose store-generated key
            // still has its default value to EntityState.Added. But for a pure composite NATURAL key entity
            // (no store-generated key at all — e.g. SecurityHistoricalValue, keyed on
            // {SecurityId, Type, Date}), EF takes Update() literally and emits an UPDATE statement that
            // matches 0 rows (the row doesn't exist yet), throwing a DbUpdateConcurrencyException on every
            // first-time insert. Add(...) is correct for a genuinely-new entity regardless of key shape.
            AddGraphRespectingExistingKeys(entity);
        }
        else
        {
            foreach (var keyPropertyInfo in _keyPropertyInfos)
            {
                var val = keyPropertyInfo.GetValue(existingEntity);
                keyPropertyInfo.SetValue(entity, val);
            }
        }
        if (this._context is MultiTenantDbContext mtCtx)
        {
            mtCtx.UpdateEntityForMultiTenancy(entity);
        }
        if (existingEntity != null && !doNotUpdateIfExists)
        {
            contextSet.Update(entity);
        }
    }

    // contextSet.Add(entity) marks entity AND every not-yet-tracked entity reachable through its navigation
    // properties as Added — "unless they are already being tracked" (EF Core's own documented Add() contract).
    // A macro that builds a new entity by pointing a reference navigation at an object returned from an
    // EARLIER, already-completed EfCoreSave call (a common pattern: look up or save a "type"/"parent" row
    // first, then reuse that in-memory object as the .TypeNavigation of many child rows saved afterwards)
    // hits this the moment that earlier save ran against a DIFFERENT DbContext instance than this one — the
    // referenced object already has a real, non-default database key, but THIS context has never seen it, so
    // Add() happily tries to INSERT it a second time and throws a UNIQUE-constraint violation on its Id.
    // Production never notices because one ASP.NET request shares a single DbContext for its whole unit of
    // work, so the earlier save's tracked instance is still tracked when the later save runs. A caller whose
    // DbContext genuinely is scoped per logical operation (e.g. a disconnected multi-DbContext test harness)
    // hits it for real — confirmed running BDLCashMovImportEtl end-to-end, 2026-07-29:
    // `Classification { ClassificationType = <already-saved SecurityClassificationType> }` re-inserted its
    // already-existing ClassificationType, throwing "UNIQUE constraint failed: ClassificationType.Id".
    // Fix: ChangeTracker.TrackGraph — EF Core's own built-in API for exactly this "disconnected graph, decide
    // each node's state from whether it already has a key" scenario (Microsoft's own docs use the identical
    // `node.Entry.State = node.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added;` recipe for
    // attaching a graph that arrived from outside the current DbContext), used here in place of
    // contextSet.Add(entity): a REFERENCED node whose primary key is already populated is marked Unchanged —
    // it's assumed to already exist — instead of being re-inserted. The ROOT entity is force-Added regardless
    // of IsKeySet: this class already proved it's new via the `existingEntity == null` pivot-key lookup above,
    // and unlike the reference data this fix targets (e.g. ClassificationType, an EF-generated surrogate Id),
    // some root entities use a purely NATURAL composite key with no store-generated column at all (e.g.
    // SecurityHistoricalValue, keyed on {SecurityId, Type, Date} — see the DbUpdateConcurrencyException comment
    // above) whose key properties are always populated by the caller before save, new row or not, so IsKeySet
    // is true even for a genuinely brand-new row — applying the IsKeySet heuristic to the root as well as its
    // references would silently turn this back into the exact "UPDATE affects 0 rows" bug this method's
    // sibling fix (above) exists to prevent.
    private void AddGraphRespectingExistingKeys(T entity)
        => _context.ChangeTracker.TrackGraph(entity, node =>
            node.Entry.State = ReferenceEquals(node.Entry.Entity, entity)
                ? EntityState.Added
                : node.Entry.IsKeySet ? EntityState.Unchanged : EntityState.Added);
}




