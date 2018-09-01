using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation.Visitors
{
    public class NewInstanceVisitor<TIn> : ExpressionVisitor
    {
        public List<Aggregator<TIn>> Aggregators { get; } = new List<Aggregator<TIn>>();
        protected override Expression VisitNew(NewExpression node)
        {
            for (int i = 0; i < node.Members.Count; i++)
            {
                var argument = node.Arguments[i];
                ValueAggregatorInspector<TIn> vis = new ValueAggregatorInspector<TIn>();
                vis.Visit(argument);
                var member = node.Members[i] as PropertyInfo;
                this.Aggregators.Add(new Aggregator<TIn>(vis.AggregationInstanceType, vis.SourcePropertyInfo, member));
            }
            return null;
        }
    }
}
