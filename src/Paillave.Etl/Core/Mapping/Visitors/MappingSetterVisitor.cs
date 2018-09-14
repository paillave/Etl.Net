using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Mapping.Visitors
{
    public class MappingSetterVisitor : ExpressionVisitor, IFieldMapper
    {
        private static readonly Type _fieldMapperType = typeof(IFieldMapper);
        public MappingSetterDefinition MappingSetter = new MappingSetterDefinition();

        public T ToColumn<T>(int columnIndex)
        {
            this.MappingSetter.ColumnIndex = columnIndex;
            return default;
        }

        public T ToColumn<T>(string columnName)
        {
            this.MappingSetter.ColumnName = columnName;
            return default;
        }

        public T ToColumn<T>(int columnIndex, int size)
        {
            this.MappingSetter.ColumnIndex = columnIndex;
            this.MappingSetter.Size = size;
            return default;
        }

        public T ToCulturedDateColumn<T>(int columnIndex, string cultureName)
        {
            this.MappingSetter.CultureName = cultureName;
            this.MappingSetter.ColumnIndex = columnIndex;
            return default;
        }

        public T ToCulturedDateColumn<T>(string columnName, string cultureName)
        {
            this.MappingSetter.CultureName = cultureName;
            this.MappingSetter.ColumnName = columnName;
            return default;
        }

        public T ToCulturedDateColumn<T>(int columnIndex, string cultureName, string dateFormat)
        {
            this.MappingSetter.CultureName = cultureName;
            this.MappingSetter.DateFormat = dateFormat;
            this.MappingSetter.ColumnIndex = columnIndex;
            return default;
        }

        public T ToCulturedDateColumn<T>(string columnName, string cultureName, string dateFormat)
        {
            this.MappingSetter.CultureName = cultureName;
            this.MappingSetter.DateFormat = dateFormat;
            this.MappingSetter.ColumnName = columnName;
            return default;
        }

        public T ToCulturedDateColumn<T>(int columnIndex, int size, string cultureName)
        {
            this.MappingSetter.CultureName = cultureName;
            this.MappingSetter.Size = size;
            this.MappingSetter.ColumnIndex = columnIndex;
            return default;
        }

        public T ToCulturedDateColumn<T>(int columnIndex, int size, string cultureName, string dateFormat)
        {
            this.MappingSetter.CultureName = cultureName;
            this.MappingSetter.Size = size;
            this.MappingSetter.DateFormat = dateFormat;
            this.MappingSetter.ColumnIndex = columnIndex;
            return default;
        }

        public T ToDateColumn<T>(int columnIndex, string dateFormat)
        {
            this.MappingSetter.DateFormat = dateFormat;
            this.MappingSetter.ColumnIndex = columnIndex;
            return default;
        }

        public T ToDateColumn<T>(string columnName, string dateFormat)
        {
            this.MappingSetter.DateFormat = dateFormat;
            this.MappingSetter.ColumnName = columnName;
            return default;
        }

        public T ToDateColumn<T>(int columnIndex, int size, string dateFormat)
        {
            this.MappingSetter.DateFormat = dateFormat;
            this.MappingSetter.ColumnIndex = columnIndex;
            return default;
        }

        public T ToNumberColumn<T>(int columnIndex, string decimalSeparator)
        {
            this.MappingSetter.DecimalSeparator = decimalSeparator;
            this.MappingSetter.ColumnIndex = columnIndex;
            return default;
        }

        public T ToNumberColumn<T>(string columnName, string decimalSeparator)
        {
            this.MappingSetter.DecimalSeparator = decimalSeparator;
            this.MappingSetter.ColumnName = columnName;
            return default;
        }

        public T ToNumberColumn<T>(int columnIndex, int size, string decimalSeparator)
        {
            this.MappingSetter.DecimalSeparator = decimalSeparator;
            this.MappingSetter.ColumnIndex = columnIndex;
            return default;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var methodInfo = node.Method;
            methodInfo.Invoke(this, node.Arguments.Cast<ConstantExpression>().Select(i => i.Value).ToArray());
            return null;
        }
    }
}
