using System;

namespace Paillave.Etl.GraphApi.Provider.Writers;
public abstract class ValueWriterBase<T> : IValueWriter
{
    public bool Handles(Type type)
    {
        return typeof(T) == type;
    }

    public abstract string Write(object value, ODataExpressionConverterSettings settings);
}
