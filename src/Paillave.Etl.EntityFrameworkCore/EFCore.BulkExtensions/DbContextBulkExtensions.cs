using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EFCore.BulkExtensions
{
    public static class DbContextBulkExtensions
    {
        public static void BulkSave<T>(this DbContext context, IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            if (entities.Count == 0) return;
            TableInfo tableInfo = TableInfo.CreateInstance(context, entities, bulkConfig);
            SqlBulkOperation.Merge(context, entities, tableInfo);
        }

        public static async Task BulkSaveAsync<T>(this DbContext context, IList<T> entities, BulkConfig bulkConfig = null) where T : class
        {
            if (entities.Count == 0) return;
            TableInfo tableInfo = TableInfo.CreateInstance(context, entities, bulkConfig);
            await SqlBulkOperation.MergeAsync(context, entities, tableInfo);
        }
    }
}
