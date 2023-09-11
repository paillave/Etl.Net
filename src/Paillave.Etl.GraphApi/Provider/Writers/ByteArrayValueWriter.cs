using System;

namespace Paillave.Etl.GraphApi.Provider.Writers;
public class ByteArrayValueWriter : ValueWriterBase<byte[]>
{
    public override string Write(object value, ODataExpressionConverterSettings settings)
    {
        var base64 = Convert.ToBase64String((byte[])value);
        return string.Format("X'{0}'", base64);
    }
}
