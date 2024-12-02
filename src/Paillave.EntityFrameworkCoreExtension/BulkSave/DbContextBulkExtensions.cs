using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave;

public static class DbContextBulkExtensions
{
    public static void BulkSave<T>(this DbContext context, IList<T> entities, CancellationToken cancellationToken, Expression<Func<T, object>>? pivotKey = null, bool doNotUpdateIfExists = false, bool insertOnly = false) where T : class
    {
        var pivotKeys = pivotKey == null ? new Expression<Func<T, object>>[0] : new[] { pivotKey };
        BulkSaveEngineBase<T> bulkSaveEngine = new BulkSaveEngine<T>(context, pivotKeys);
        bulkSaveEngine.Save(entities, cancellationToken, doNotUpdateIfExists, insertOnly);
    }
    public static void BulkSave<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>>[] pivotKeys, CancellationToken cancellationToken, bool doNotUpdateIfExists = false, bool insertOnly = false) where T : class
    {
        BulkSaveEngineBase<T> bulkSaveEngine = new BulkSaveEngine<T>(context, pivotKeys);
        bulkSaveEngine.Save(entities, cancellationToken, doNotUpdateIfExists, insertOnly);
    }
    public static void BulkUpdate<TEntity, TSource>(this DbContext context, IList<TSource> sources, Expression<Func<TSource, TEntity>> updateKey, Expression<Func<TSource, TEntity>> updateValues) where TEntity : class
    {
        BulkUpdateEngineBase<TEntity, TSource> bulkUpdateEngine = new BulkUpdateEngine<TEntity, TSource>(context, updateKey, updateValues);
        bulkUpdateEngine.Update(sources);
    }
}
