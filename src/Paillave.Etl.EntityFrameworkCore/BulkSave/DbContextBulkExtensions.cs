using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.EntityFrameworkCore.BulkSave
{
    public static class DbContextBulkExtensions
    {
        public static void BulkSave<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>> pivotKey = null, bool doNotUpdateIfExists = false) where T : class
        {
            BulkSaveEngineBase<T> bulkSaveEngine = new SqlServerBulkSaveEngine<T>(context, pivotKey);
            bulkSaveEngine.Save(entities, doNotUpdateIfExists);
        }
        public static void BulkUpdate<TEntity, TSource>(this DbContext context, IList<TSource> sources, Expression<Func<TSource, TEntity>> updateKey, Expression<Func<TSource, TEntity>> updateValues) where TEntity : class
        {
            BulkUpdateEngineBase<TEntity, TSource> bulkUpdateEngine = new SqlServerBulkUpdateEngine<TEntity, TSource>(context, updateKey, updateValues);
            bulkUpdateEngine.Update(sources);
        }
    }
}
