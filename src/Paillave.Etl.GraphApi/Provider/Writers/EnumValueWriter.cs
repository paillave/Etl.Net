using System;

namespace Paillave.Etl.GraphApi.Provider.Writers;

public class EnumValueWriter : IValueWriter
{
    public bool Handles(Type type)
    {
        return type.IsEnum;
    }

    public string Write(object value, ODataExpressionConverterSettings settings)
    {
        var enumType = value.GetType();
        string assembly;
        string typename;
        settings.SerializationBinder.BindToName(enumType, out assembly, out typename);

        return string.Format("{0}'{1}'", typename, value);
    }
}
