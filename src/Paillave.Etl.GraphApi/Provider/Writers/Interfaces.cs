using System;
using System.Linq.Expressions;

namespace Paillave.Etl.GraphApi.Provider.Writers;
public interface IMethodCallWriter
{
    bool CanHandle(MethodCallExpression expression);

    string Handle(MethodCallExpression expression, Func<Expression, string> expressionWriter, ODataExpressionConverterSettings settings);
}

public interface IMemberCallWriter
{
    bool CanHandle(MemberExpression expression);

    string Handle(MemberExpression expression, ODataExpressionConverterSettings settings);
}

public interface IValueWriter
{
    bool Handles(Type type);

    string Write(object value, ODataExpressionConverterSettings settings);
}

