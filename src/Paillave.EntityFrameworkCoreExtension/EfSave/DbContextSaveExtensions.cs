using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.EntityFrameworkCoreExtension.EfSave;

public static class DbContextSaveExtensions
{
    public static Task EfSaveAsync<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>> pivotKey, bool doNotUpdateIfExists = false, bool insertOnly = false, CancellationToken cancellationToken = default) where T : class
        => EfSaveAsync(context, entities, [pivotKey], doNotUpdateIfExists, insertOnly, cancellationToken);
    public static async Task EfSaveAsync<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>>[] pivotKeys, bool doNotUpdateIfExists = false, bool insertOnly = false, CancellationToken cancellationToken = default) where T : class
    {
        EfSaveEngine<T> efSaveEngine = new(context, cancellationToken, pivotKeys);
        await efSaveEngine.SaveAsync(entities, doNotUpdateIfExists, insertOnly);
    }
    public static async Task EfSaveAsync<T>(this DbContext context, IList<T> entities, Expression<Func<T, T, bool>> pivotCondition, bool doNotUpdateIfExists = false, bool insertOnly = false, CancellationToken cancellationToken = default) where T : class
    {
        EfSaveEngine<T> efSaveEngine = new(context, cancellationToken, pivotCondition);
        await efSaveEngine.SaveAsync(entities, doNotUpdateIfExists, insertOnly);
    }
}
