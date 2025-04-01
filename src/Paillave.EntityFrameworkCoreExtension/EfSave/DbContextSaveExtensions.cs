using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.EntityFrameworkCoreExtension.EfSave;

public static class DbContextSaveExtensions
{
    public static Task EfSaveAsync<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>> pivotKey, bool doNotUpdateIfExists = false, bool insertOnly = false) where T : class
        => EfSaveAsync(context, entities, new Expression<Func<T, object>>[] { pivotKey }, CancellationToken.None, doNotUpdateIfExists, insertOnly);
    public static Task EfSaveAsync<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>> pivotKey, CancellationToken cancellationToken, bool doNotUpdateIfExists = false, bool insertOnly = false) where T : class
        => EfSaveAsync(context, entities, new Expression<Func<T, object>>[] { pivotKey }, cancellationToken, doNotUpdateIfExists, insertOnly);
    public static Task EfSaveAsync<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>>[] pivotKeys, bool doNotUpdateIfExists = false, bool insertOnly = false) where T : class
        => EfSaveAsync<T>(context, entities, pivotKeys, CancellationToken.None, doNotUpdateIfExists, insertOnly);
    public static async Task EfSaveAsync<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>>[] pivotKeys, CancellationToken? cancellationToken = null, bool doNotUpdateIfExists = false, bool insertOnly = false) where T : class
    {
        if (cancellationToken == null)
        {
            cancellationToken = CancellationToken.None;
        }
        EfSaveEngine<T> efSaveEngine = new EfSaveEngine<T>(context, cancellationToken.Value, pivotKeys);
        await efSaveEngine.SaveAsync(entities, doNotUpdateIfExists, insertOnly);
    }

    public static Task EfSaveAsync<T>(this DbContext context, IList<T> entities, Expression<Func<T, T, bool>> pivotCondition, bool doNotUpdateIfExists = false, bool insertOnly = false) where T : class
        => EfSaveAsync(context, entities, pivotCondition, CancellationToken.None, doNotUpdateIfExists, insertOnly);
    public static async Task EfSaveAsync<T>(this DbContext context, IList<T> entities, Expression<Func<T, T, bool>> pivotCondition, CancellationToken? cancellationToken = null, bool doNotUpdateIfExists = false, bool insertOnly = false) where T : class
    {
        if (cancellationToken == null)
        {
            cancellationToken = CancellationToken.None;
        }
        EfSaveEngine<T> efSaveEngine = new EfSaveEngine<T>(context, cancellationToken.Value, pivotCondition);
        await efSaveEngine.SaveAsync(entities, doNotUpdateIfExists, insertOnly);
    }
}
