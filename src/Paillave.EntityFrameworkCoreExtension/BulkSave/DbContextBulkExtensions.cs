using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Paillave.EntityFrameworkCoreExtension.BulkSave.SqlServer;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave
{
    public static class DbContextBulkExtensions
    {
        public static void BulkSave<T>(this DbContext context, IList<T> entities, CancellationToken cancellationToken, Expression<Func<T, object>> pivotKey = null, bool doNotUpdateIfExists = false, bool insertOnly = false) where T : class
        {
            BulkSaveEngineBase<T> bulkSaveEngine = new BulkSaveEngine<T>(context, pivotKey);
            bulkSaveEngine.Save(entities, cancellationToken, doNotUpdateIfExists, insertOnly);
        }
        public static void BulkSave<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>>[] pivotKeys, CancellationToken cancellationToken, bool doNotUpdateIfExists = false, bool insertOnly = false, bool identityInsert = false ) where T : class
        {
            BulkSaveEngineBase<T> bulkSaveEngine = new BulkSaveEngine<T>(context, pivotKeys);
            bulkSaveEngine.Save(entities, cancellationToken, doNotUpdateIfExists, insertOnly, identityInsert );
        }
        public static void BulkUpdate<TEntity, TSource>(this DbContext context, IList<TSource> sources, Expression<Func<TSource, TEntity>> updateKey, Expression<Func<TSource, TEntity>> updateValues) where TEntity : class
        {
            BulkUpdateEngineBase<TEntity, TSource> bulkUpdateEngine = new BulkUpdateEngine<TEntity, TSource>(context, updateKey, updateValues);
            bulkUpdateEngine.Update(sources);
        }
    }
}
