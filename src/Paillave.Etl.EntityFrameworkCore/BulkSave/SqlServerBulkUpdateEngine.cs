using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Paillave.Etl.EntityFrameworkCore.BulkSave
{
    public class SqlServerBulkUpdateEngine<TEntity, TSource> : BulkUpdateEngineBase<TEntity, TSource> where TEntity : class
    {
        public SqlServerBulkUpdateEngine(DbContext context, Expression<Func<TSource, TEntity>> updateKey, Expression<Func<TSource, TEntity>> updateValues)
            : base(context, updateKey, updateValues) { }

        protected override UpdateContextQueryBase<TSource> CreateUpdateContextQueryInstance(DbContext context, string schema, string table, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, IEntityType baseType, IDictionary<string, MemberInfo> propertiesGetter)
            => new SqlServerUpdateContextQuery<TSource>(context, schema, table, propertiesToUpdate, propertiesForPivot, propertiesToBulkLoad, baseType, propertiesGetter);
    }
}
