namespace Paillave.Etl.GraphApi.Provider.Writers;
public class BooleanValueWriter : ValueWriterBase<bool>
{
    public override string Write(object value, ODataExpressionConverterSettings settings)
    {
        var boolean = (bool)value;

        return boolean ? "true" : "false";
    }
}
