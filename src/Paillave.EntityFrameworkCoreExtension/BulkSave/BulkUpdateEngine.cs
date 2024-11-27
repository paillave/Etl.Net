﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
// using Paillave.EntityFrameworkCoreExtension.BulkSave.Postgres;
using Paillave.EntityFrameworkCoreExtension.BulkSave.SqlServer;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave
{
    public class BulkUpdateEngine<TEntity, TSource>(DbContext context, Expression<Func<TSource,
        TEntity>> updateKey, Expression<Func<TSource,
            TEntity>> updateValues) : BulkUpdateEngineBase<TEntity, TSource>(context, updateKey, updateValues) where TEntity : class
    {
        protected override UpdateContextQueryBase<TSource> CreateUpdateContextQueryInstance(DbContext context, string schema, string table, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, IEntityType baseType, IDictionary<string, MemberInfo> propertiesGetter)
        {
            if (context.Database.IsSqlServer())
                return new SqlServerUpdateContextQuery<TSource>(context, schema, table, propertiesToUpdate, propertiesForPivot, propertiesToBulkLoad, baseType, propertiesGetter, base.StoreObject);
            // if (context.Database.IsNpgsql())
            //     return new PostgresUpdateContextQuery<TSource>(context, schema, table, propertiesToUpdate, propertiesForPivot, propertiesToBulkLoad, baseType, propertiesGetter);
            throw new Exception("unsupported provider");
        }
    }
}
