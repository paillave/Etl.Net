using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Paillave.Etl.EntityFrameworkCore.BulkSave
{
    public abstract class ContextQueryBase<T> where T : class
    {
        protected string Table { get; }
        protected string StagingId { get; } = Guid.NewGuid().ToString().Substring(0, 8);
        protected string Schema { get; }
        protected List<IEntityType> EntityTypes { get; }

        /// <summary>
        /// Any column except computed
        /// </summary>
        protected List<IProperty> PropertiesToInsert { get; }
        /// <summary>
        /// any column except pivot, computed
        /// </summary>
        protected List<IProperty> PropertiesToUpdate { get; }
        /// <summary>
        /// pivot columns
        /// </summary>
        protected List<IProperty> PropertiesForPivot { get; }

        /// <summary>
        /// Any column except computed that is not pivot
        /// </summary>
        protected List<IProperty> PropertiesToBulkLoad { get; }

        protected DbContext Context { get; }
        public ContextQueryBase(DbContext context, string schema, string table, List<IProperty> propertiesToInsert, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, List<IEntityType> entityTypes)
        {
            this.PropertiesToInsert = propertiesToInsert;
            this.PropertiesToUpdate = propertiesToUpdate;
            this.PropertiesForPivot = propertiesForPivot;
            this.PropertiesToBulkLoad = propertiesToBulkLoad;
            this.Schema = schema;
            this.Table = table;
            this.Context = context;
            this.EntityTypes = entityTypes;
        }

        /// <summary>
        /// Create the staging that is meant to receive the raw bulk load
        /// </summary>
        public abstract void CreateStagingTable();
        /// <summary>
        /// Create the output staging table that is mean to receive the result of the merge in the right order to update entities
        /// </summary>
        public abstract void CreateOutputStagingTable();
        /// <summary>
        /// save entities in the staging table
        /// </summary>
        /// <param name="entities">entities that will saved in staging</param>
        public abstract void BulkSaveInStaging(IEnumerable<T> entities);
        /// <summary>
        /// merge the staging table in the target table 
        /// </summary>
        public abstract void MergeFromStaging();
        public abstract void IndexStagingTable();
        public abstract void IndexOutputStagingTable();
        public abstract IList<T> GetOutputStaging();
        public abstract void DeleteStagingTable();
        public abstract void DeleteOutputStagingTable();
        protected IEnumerable<T> QueryOutputTable(string sqlQuery)
        {
            var compiled = EF.CompileQuery(GetQueryExpression(sqlQuery));
            var result = compiled(Context);
            return result;
        }
        //.OrderBy(i=>EF.Property<int>(i,))
        public Expression<Func<DbContext, IQueryable<T>>> GetQueryExpression(string sqlQuery)
            => (ctx) => ctx.Set<T>().IgnoreQueryFilters().FromSql(sqlQuery).AsNoTracking();
        //private Expression<Func<DbContext, IQueryable<T>>> OrderBy(Expression<Func<DbContext, IQueryable<T>>> source)
        //{
        //    Type entityType = typeof(T);
        //    PropertyInfo property = entityType.GetProperty(ordering);
        //    ParameterExpression parameter = Expression.Parameter(entityType);
        //    MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);
        //    LambdaExpression orderByExp = Expression.Lambda(propertyAccess, parameter);
        //    MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderBy", new Type[] { entityType, property.PropertyType }, source.Body, Expression.Quote(orderByExp));
        //    return Expression.Lambda<Func<DbContext, IQueryable<T>>>(resultExp, source.Parameters);
        //}
    }
}
