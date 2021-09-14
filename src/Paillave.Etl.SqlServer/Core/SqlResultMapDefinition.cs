using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.SqlServer.Core
{
    public static class SqlResultMapDefinition
    {
        public static SqlResultMapDefinition<T> Create<T>(Expression<Func<ISqlResultMapper, T>> expression) => new SqlResultMapDefinition<T>().WithMap(expression);
        public static SqlResultMapDefinition<T> Create<T>() => new SqlResultMapDefinition<T>().WithDefaultMap();
    }
    public class SqlResultMapDefinition<T>
    {
        private IList<SqlResultFieldDefinition> _fieldDefinitions = new List<SqlResultFieldDefinition>();
        public IList<SqlResultFieldDefinition> GetDefinitions() => this._fieldDefinitions.ToList();
        public SqlResultMapDefinition<T> WithMap(Expression<Func<ISqlResultMapper, T>> expression)
        {
            MapperVisitor vis = new MapperVisitor();
            vis.Visit(expression);
            foreach (var item in vis.MappingSetters)
                this.SetFieldDefinition(item);
            return this;
        }
        public SqlResultMapDefinition<T> WithDefaultMap()
        {
            foreach (var item in typeof(T).GetProperties().Select((propertyInfo, index) => new { propertyInfo = propertyInfo, Position = index }))
                this.SetFieldDefinition(new SqlResultFieldDefinition
                {
                    ColumnName = item.propertyInfo.Name,
                    PropertyInfo = item.propertyInfo
                });
            return this;
        }
        private void SetFieldDefinition(SqlResultFieldDefinition fieldDefinition)
        {
            var existingFieldDefinition = _fieldDefinitions.FirstOrDefault(i => i.PropertyInfo.Name == fieldDefinition.PropertyInfo.Name);
            if (existingFieldDefinition == null)
            {
                _fieldDefinitions.Add(fieldDefinition);
            }
            else
            {
                if (fieldDefinition.ColumnName != null) existingFieldDefinition.ColumnName = fieldDefinition.ColumnName;
            }
        }
    }
}
