using System;
using System.Linq.Expressions;

namespace Paillave.Etl.GraphApi.Provider.Writers;
public class EqualsMethodWriter : IMethodCallWriter
{
    public bool CanHandle(MethodCallExpression expression)
    {
        return expression.Method.Name == "Equals";
    }

    public string Handle(MethodCallExpression expression, Func<Expression, string> expressionWriter, ODataExpressionConverterSettings settings)
    {
        return string.Format(
            "{0} eq {1}",
            expressionWriter(expression.Object),
            expressionWriter(expression.Arguments[0]));
    }
}
