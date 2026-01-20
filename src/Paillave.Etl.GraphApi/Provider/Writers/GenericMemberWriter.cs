using System;
using System.Linq.Expressions;

namespace Paillave.Etl.GraphApi.Provider.Writers;
public class GenericMemberWriter(Type type, string methodName, Func<ODataExpressionConverterSettings, string> odataFunction) : IMemberCallWriter
{
    readonly Type type = type;
    readonly string methodName = methodName;
    readonly Func<ODataExpressionConverterSettings, string> odataFunction = odataFunction;

    public GenericMemberWriter(Type type, string methodName, string odataFunction)
        : this(type, methodName, (s) => odataFunction)
    {

    }

    public bool CanHandle(MemberExpression expression)
    {
        return expression.Member.DeclaringType == type
               && methodName.Equals(expression.Member.Name, StringComparison.Ordinal);
    }

    public string Handle(MemberExpression expression, ODataExpressionConverterSettings settings)
    {
        return odataFunction(settings);
    }
}
