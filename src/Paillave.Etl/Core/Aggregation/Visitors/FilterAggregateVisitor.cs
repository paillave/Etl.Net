using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation.Visitors
{
    public class FilterAggregateVisitor<TIn> : ExpressionVisitor
    {
        public PropertyInfo? FilteredPropertyInfo { get; private set; } = null;
        public object? Filter { get; private set; }
        public MethodCallExpression? GetValueExpression { get; private set; }
        protected override Expression? VisitMethodCall(MethodCallExpression node)
        {
            GetValueExpression = node.Arguments[0] as MethodCallExpression;
            EqualityAggregateVisitor<TIn> vis = new EqualityAggregateVisitor<TIn>();
            vis.Visit(node.Arguments[1]);
            this.FilteredPropertyInfo = vis.FilteredPropertyInfo;
            this.Filter = vis.Filter;
            return null;
        }
    }
}
