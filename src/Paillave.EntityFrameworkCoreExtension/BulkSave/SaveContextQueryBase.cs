using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave
{
    public abstract class SaveContextQueryBase<T> where T : class
    {
        protected string Table { get; }
        protected StoreObjectIdentifier StoreObject { get; }
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
        protected List<List<IProperty>> PropertiesForPivotSet { get; }

        /// <summary>
        /// Any column except computed that is not pivot
        /// </summary>
        protected List<IProperty> PropertiesToBulkLoad { get; }

        protected CancellationToken CancellationToken { get; }

        protected DbContext Context { get; }
        public SaveContextQueryBase(DbContext context, string schema, string table, List<IProperty> propertiesToInsert, List<IProperty> propertiesToUpdate, List<List<IProperty>> propertiesForPivotSet, List<IProperty> propertiesToBulkLoad, List<IEntityType> entityTypes, CancellationToken cancellationToken, StoreObjectIdentifier storeObject)
        {
            this.StoreObject = storeObject;
            this.CancellationToken = cancellationToken;
            this.PropertiesToInsert = propertiesToInsert;
            this.PropertiesToUpdate = propertiesToUpdate;
            this.PropertiesForPivotSet = propertiesForPivotSet;
            this.PropertiesToBulkLoad = propertiesToBulkLoad;
            this.Schema = schema;
            this.Table = table;
            this.Context = context;
            this.EntityTypes = entityTypes;
        }

        /// <summary>
        /// Create the staging that is meant to receive the raw bulk load
        /// </summary>
        public abstract int CreateStagingTable();
        /// <summary>
        /// Create the output staging table that is mean to receive the result of the merge in the right order to update entities
        /// </summary>
        public abstract int CreateOutputStagingTable();
        /// <summary>
        /// save entities in the staging table
        /// </summary>
        /// <param name="entities">entities that will saved in staging</param>
        public abstract void BulkSaveInStaging(IEnumerable<T> entities);
        /// <summary>
        /// merge the staging table in the target table 
        /// </summary>
        public abstract void MergeFromStaging(bool doNotUpdateIfExists = false);
        public abstract void InsertFromStaging();
        public abstract void IndexStagingTable(List<IProperty> propertiesForPivot);
        public abstract void IndexOutputStagingTable();
        public abstract DataTable GetOutputStaging();
        public abstract DataTable GetOutputStagingForComputedColumns();
        public abstract void DeleteStagingTable();
        public abstract void DeleteOutputStagingTable();
        protected DataTable StrictlyExecuteSql(string sqlQuery)
        {
            var cmd = this.Context.Database.GetDbConnection().CreateCommand();
            if (cmd.Connection.State == ConnectionState.Closed)
                cmd.Connection.Open();
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.CommandText = sqlQuery;
            var dataReader = cmd.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(dataReader);
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/queries-in-linq-to-dataset
            return dataTable;
        }
    }
}
