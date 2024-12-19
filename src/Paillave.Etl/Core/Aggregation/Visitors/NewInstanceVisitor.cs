using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation.Visitors
{
    public class NewInstanceVisitor<TIn, TValue, TAggregator> : ExpressionVisitor where TAggregator : AggregatorBase<TIn, TValue>
    {
        public List<TAggregator> Aggregators { get; } = new List<TAggregator>();
        private TAggregator CreateInstance(Type aggregatorInstanceType, PropertyInfo sourcePropertyInfo, PropertyInfo targetPropertyInfo)
            => (TAggregator)Activator.CreateInstance(typeof(TAggregator), new object[] { aggregatorInstanceType, sourcePropertyInfo, targetPropertyInfo });
        protected override Expression? VisitNew(NewExpression node)
        {
            for (int i = 0; i < node.Members.Count; i++)
            {
                var argument = node.Arguments[i];
                MappingSetterVisitor<TValue> vis = new MappingSetterVisitor<TValue>();
                vis.Visit(argument);
                var member = node.Members[i] as PropertyInfo;
                if (member == null)
                    throw new InvalidOperationException("Only properties are supported in the aggregation");
                if (vis.AggregationInstanceType == null)
                    throw new InvalidOperationException("Only properties with AggregationInstanceAttribute are supported in the aggregation");
                if (vis.SourcePropertyInfo == null)
                    throw new InvalidOperationException("Only properties with a getter are supported in the aggregation");
                var agg = this.CreateInstance(vis.AggregationInstanceType, vis.SourcePropertyInfo, member);
                if (vis.FilteredPropertyInfo != null && vis.Filter != null)
                    agg.SetFilter(vis.FilteredPropertyInfo, vis.Filter);
                this.Aggregators.Add(agg);
            }
            return null;
        }
    }
}
