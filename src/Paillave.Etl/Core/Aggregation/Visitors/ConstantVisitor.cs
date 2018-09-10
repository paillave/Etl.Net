using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Aggregation.Visitors
{
    public class ConstantVisitor<TIn> : ExpressionVisitor
    {
        public object Filter { get; private set; }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.Filter=node.Value;
            return null;
        }
    }
}
