﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
// using Paillave.EntityFrameworkCoreExtension.BulkSave.Postgres;
using Paillave.EntityFrameworkCoreExtension.BulkSave.SqlServer;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave
{
    public class BulkSaveEngine<T>(DbContext context, params Expression<Func<T,
        object>>[] pivotKeys) : BulkSaveEngineBase<T>(context, pivotKeys) where T : class
    {
        protected override SaveContextQueryBase<T> CreateSaveContextQueryInstance(DbContext context, string schema, string table, List<IProperty> propertiesToInsert, List<IProperty> propertiesToUpdate, List<List<IProperty>> propertiesForPivotSet, List<IProperty> propertiesToBulkLoad, List<IEntityType> entityTypes, CancellationToken cancellationToken)
        {
            if (context.Database.IsSqlServer())
                return new SqlServerSaveContextQuery<T>(context, schema, table, propertiesToInsert, propertiesToUpdate, propertiesForPivotSet, propertiesToBulkLoad, entityTypes, cancellationToken, base.StoreObject);
            // if (context.Database.IsNpgsql())
            //     return new PostgresSaveContextQuery<T>(context, schema, table, propertiesToInsert, propertiesToUpdate, propertiesForPivotSet, propertiesToBulkLoad, entityTypes);
            throw new Exception("unsupported provider");
        }
    }
}
