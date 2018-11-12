using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Mapping.Visitors
{
    public class MappingSetterVisitor : ExpressionVisitor
    {
        public MappingSetterDefinition MappingSetter = null;
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            DummyFieldMapper dummyFieldMapper = new DummyFieldMapper();
            var methodInfo = node.Method;
            methodInfo.Invoke(dummyFieldMapper, node.Arguments.Cast<ConstantExpression>().Select(i => i.Value).ToArray());
            this.MappingSetter = dummyFieldMapper.MappingSetter;
            return null;
        }
    }
}
