using System;

namespace Paillave.Etl.GraphApi.Provider.Writers;
public class GuidValueWriter : IValueWriter
{
    public bool Handles(Type type)
    {
        return type == typeof(Guid);
    }

    public string Write(object value, ODataExpressionConverterSettings settings)
    {
        Guid guid = (Guid)value;

        return string.Format("guid'{0}'", guid.ToString().ToLower());
    }
}
