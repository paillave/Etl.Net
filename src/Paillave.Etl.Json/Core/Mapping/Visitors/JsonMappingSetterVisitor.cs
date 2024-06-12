using System.Linq.Expressions;

namespace Paillave.Etl.Json.Core.Mapping.Visitors
{
    public class JsonMappingSetterVisitor : ExpressionVisitor
    {
        public JsonFieldDefinition MappingSetter = null;
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
