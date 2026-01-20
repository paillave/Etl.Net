using System;

namespace Paillave.Etl.GraphApi.Provider.Writers;
public class GenericValueWriter(Type type, Func<object, ODataExpressionConverterSettings, string> odataFunction) : IValueWriter
{
    readonly Type type = type;
    readonly Func<object, ODataExpressionConverterSettings, string> odataFunction = odataFunction;

    public GenericValueWriter(Type type, Func<object, string> odataFunction) : this(type, (a, b) => odataFunction(a))
    {
    }

    public bool Handles(Type typeToTest)
    {
        return type == typeToTest;
    }

    public string Write(object value, ODataExpressionConverterSettings settings)
    {
        return odataFunction(value, settings);
    }
}
