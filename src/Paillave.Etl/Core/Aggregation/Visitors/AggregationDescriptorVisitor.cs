using System.Collections.Generic;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.Aggregation.Visitors;

public class AggregationDescriptorVisitor<TIn, TValue, TAggregator> : ExpressionVisitor where TAggregator : AggregatorBase<TIn, TValue>
{
    public List<TAggregator>? AggregationsToProcess { get; private set; }
    protected override Expression? VisitLambda<T>(Expression<T> node)
    {
        NewInstanceVisitor<TIn, TValue, TAggregator> vis = new();
        vis.Visit(node.Body);
        this.AggregationsToProcess = vis.Aggregators;
        return null;
    }
}
