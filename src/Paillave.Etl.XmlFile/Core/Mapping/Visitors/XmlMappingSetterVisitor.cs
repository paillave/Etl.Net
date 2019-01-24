using Paillave.Etl.Core.Aggregation.AggregationInstances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.XmlFile.Core.Mapping.Visitors
{
    public class XmlMappingSetterVisitor : ExpressionVisitor
    {
        public XmlFieldDefinition MappingSetter = null;
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
