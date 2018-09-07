using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation.Visitors
{
    public class EqualityAggregateVisitor<TIn> : ExpressionVisitor
    {
        public PropertyInfo FilteredPropertyInfo { get; private set; } = null;
        public object Filter { get; private set; }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType != ExpressionType.Equal) return null;
            ValueToAggregateVisitor<TIn> visV = new ValueToAggregateVisitor<TIn>();
            visV.Visit(node.Left);
            this.FilteredPropertyInfo = visV.SourcePropertyInfo;
            ConstantVisitor<TIn> visC = new ConstantVisitor<TIn>();
            visC.Visit(node.Right);
            this.Filter = visC.Filter;
            return null;
        }
    }
}
