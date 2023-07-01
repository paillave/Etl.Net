using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.SqlServer.Core
{
    public class MappingSetterVisitor : ExpressionVisitor
    {
        public string ColumnName = null;
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            DummyFieldMapper dummyFieldMapper = new DummyFieldMapper();
            var methodInfo = node.Method;
            methodInfo.Invoke(dummyFieldMapper, node.Arguments.Cast<ConstantExpression>().Select(i => i.Value).ToArray());
            this.ColumnName = dummyFieldMapper.ColumnName;
            return null;
        }
    }
}
