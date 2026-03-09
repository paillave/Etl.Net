using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation.Visitors;

public class GetValueAggregatorVisitor<TIn> : ExpressionVisitor
{
    public Type? AggregationInstanceType { get; private set; }
    public PropertyInfo? SourcePropertyInfo { get; private set; }
    protected override Expression? VisitMethodCall(MethodCallExpression node)
    {
        var aggregationInstanceAttribute = node.Method.GetCustomAttribute<AggregationInstanceAttribute>();
        if (aggregationInstanceAttribute == null) throw new InvalidOperationException();
        this.AggregationInstanceType = aggregationInstanceAttribute.AggregationInstanceType;
        if (node.Arguments.Count > 0)
        {
            ValueToAggregateVisitor<TIn> vis = new();
            try
            {
                vis.Visit(node.Arguments[0]);
                this.SourcePropertyInfo = vis.SourcePropertyInfo;
            }
            catch { }
        }
        return null;
    }
}
