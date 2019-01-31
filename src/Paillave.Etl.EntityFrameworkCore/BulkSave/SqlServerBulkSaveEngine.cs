using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Paillave.Etl.EntityFrameworkCore.BulkSave
{
    public class SqlServerBulkSaveEngine<T> : BulkSaveEngineBase<T> where T : class
    {
        public SqlServerBulkSaveEngine(DbContext context, Expression<Func<T, object>> pivotKey = null) : base(context, pivotKey)
        {
        }

        protected override SaveContextQueryBase<T> CreateSaveContextQueryInstance(DbContext context, string schema, string table, List<IProperty> propertiesToInsert, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, List<IEntityType> entityTypes)
            => new SqlServerSaveContextQuery<T>(context, schema, table, propertiesToInsert, propertiesToUpdate, propertiesForPivot, propertiesToBulkLoad, entityTypes);
    }
}
