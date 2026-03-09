using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation.Visitors;

public class ValueToAggregateVisitor<TIn> : ExpressionVisitor
{
    public PropertyInfo? SourcePropertyInfo { get; private set; }
    public override Expression? Visit(Expression node)
    {
        base.Visit(node);
        if (SourcePropertyInfo == null)
            throw new NotSupportedException($"{node.NodeType} is not supported in an aggregation");
        return null;
    }
    protected override Expression? VisitMember(MemberExpression node)
    {
        this.SourcePropertyInfo = node.Member as PropertyInfo;
        return null;
    }
}
