using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation.Visitors
{
    public class MappingSetterVisitor<TIn> : ExpressionVisitor
    {
        public Type AggregationInstanceType { get; private set; }
        public PropertyInfo SourcePropertyInfo { get; private set; }
        public PropertyInfo FilteredPropertyInfo { get; private set; } = null;
        public object Filter { get; private set; }
        private bool IsFilter(MethodInfo methodInfo)
        {
            return methodInfo.Name == nameof(AggregationOperators.For);
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (IsFilter(node.Method))
            {
                FilterAggregateVisitor<TIn> visF = new FilterAggregateVisitor<TIn>();
                visF.Visit(node);
                this.Filter = visF.Filter;
                this.FilteredPropertyInfo = visF.FilteredPropertyInfo;
                node = visF.GetValueExpression;
            }
            GetValueAggregatorVisitor<TIn> vis = new GetValueAggregatorVisitor<TIn>();
            vis.Visit(node);
            this.AggregationInstanceType = vis.AggregationInstanceType;
            this.SourcePropertyInfo = vis.SourcePropertyInfo;
            return null;
        }
    }
}
