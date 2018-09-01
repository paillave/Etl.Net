using System.Collections.Generic;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.Aggregation.Visitors
{
    public class AggregationDescriptorVisitor<TIn> : ExpressionVisitor
    {
        public List<Aggregator<TIn>> AggregationsToProcess { get; private set; }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            NewInstanceVisitor<TIn> vis = new NewInstanceVisitor<TIn>();
            vis.Visit(node.Body);
            this.AggregationsToProcess = vis.Aggregators;
            return null;
        }
    }
}
