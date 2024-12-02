using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Paillave.EntityFrameworkCoreExtension.BulkSave.SqlServer;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave;

public class BulkUpdateEngine<TEntity, TSource> : BulkUpdateEngineBase<TEntity, TSource> where TEntity : class
{
    public BulkUpdateEngine(DbContext context, Expression<Func<TSource, TEntity>> updateKey, Expression<Func<TSource, TEntity>> updateValues)
        : base(context, updateKey, updateValues) { }

    protected override UpdateContextQueryBase<TSource> CreateUpdateContextQueryInstance(DbContext context, string schema, string table, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, IEntityType baseType, IDictionary<string, MemberInfo> propertiesGetter)
    {
        if (context.Database.IsSqlServer())
            return new SqlServerUpdateContextQuery<TSource>(context, schema, table, propertiesToUpdate, propertiesForPivot, propertiesToBulkLoad, baseType, propertiesGetter, base.StoreObject);
        // if (context.Database.IsNpgsql())
        //     return new PostgresUpdateContextQuery<TSource>(context, schema, table, propertiesToUpdate, propertiesForPivot, propertiesToBulkLoad, baseType, propertiesGetter);
        throw new Exception("unsupported provider");
    }
}
