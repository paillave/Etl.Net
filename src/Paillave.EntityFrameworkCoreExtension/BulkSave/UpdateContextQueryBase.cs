using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave
{
    public abstract class UpdateContextQueryBase<TSource>
    {
        protected string Table { get; }
        protected string StagingId { get; } = Guid.NewGuid().ToString().Substring(0, 8);
        protected string Schema { get; }
        protected IEntityType BaseType { get; }
        protected IDictionary<string, MemberInfo> PropertyGetters { get; }
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
        public UpdateContextQueryBase(DbContext context, string schema, string table, List<IProperty> propertiesToUpdate, List<IProperty> propertiesForPivot, List<IProperty> propertiesToBulkLoad, IEntityType baseType, IDictionary<string, MemberInfo> propertyGetters)
        {
            this.PropertiesToUpdate = propertiesToUpdate;
            this.PropertiesForPivot = propertiesForPivot;
            this.PropertiesToBulkLoad = propertiesToBulkLoad;
            this.PropertyGetters = propertyGetters;

            this.Schema = schema;
            this.Table = table;
            this.Context = context;
            this.BaseType = baseType;
        }

        /// <summary>
        /// Create the staging that is meant to receive the raw bulk load
        /// </summary>
        public abstract void CreateStagingTable();
        /// <summary>
        /// save entities in the staging table
        /// </summary>
        /// <param name="entities">entities that will saved in staging</param>
        public abstract void BulkSaveInStaging(IEnumerable<TSource> sources);
        /// <summary>
        /// merge the staging table in the target table 
        /// </summary>
        public abstract void MergeFromStaging();
        public abstract void IndexStagingTable();
        public abstract void DeleteStagingTable();
    }
}
