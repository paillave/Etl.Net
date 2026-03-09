using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.GraphApi.Provider.Writers;
public class GenericMethodWriter(Type type, string methodName, Func<string, IEnumerable<string>, ODataExpressionConverterSettings, string> odataFunction) : IMethodCallWriter
{
    readonly Type type = type;
    readonly string methodName = methodName;
    readonly Func<string, IEnumerable<string>, ODataExpressionConverterSettings, string> odataFunction = odataFunction;

    public GenericMethodWriter(Type type, string methodName, string odataFunction)
        : this(type, methodName, (obj, args, s) =>
        {
            var fullArgs = args.ToList();
            if (obj != null)
            {
                fullArgs.Insert(0, obj);
            }
            return $"{odataFunction}({string.Join(", ", fullArgs)})";
        })
    {

    }
    public GenericMethodWriter(Type type, string methodName, Func<string, IEnumerable<string>, string> odataFunction) : this(type, methodName, (a, b, c) => odataFunction(a, b))
    {
    }

    public bool CanHandle(MethodCallExpression expression)
    {
        return expression.Method.DeclaringType == type
               && methodName.Equals(expression.Method.Name, StringComparison.Ordinal);
    }

    public string Handle(MethodCallExpression expression, Func<Expression, string> expressionWriter, ODataExpressionConverterSettings settings)
    {
        var arguments = expression.Arguments.Select(x => expressionWriter(x)).ToList();
        string objectValue = null;

        if (expression.Object != null) //instance method not static
        {
            objectValue = expressionWriter(expression.Object);
        }
        return odataFunction(objectValue, arguments, settings);
    }
}
