using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.EntityFrameworkCore.BulkSave
{
    public static class DbContextBulkExtensions
    {
        public static void BulkSave<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>> pivotKey = null) where T : class
        {
            BulkSaveEngineBase<T> bulkSaveEngine = new SqlServerBulkSaveEngine<T>(context, pivotKey);
            bulkSaveEngine.Save(entities);
        }
    }
}
