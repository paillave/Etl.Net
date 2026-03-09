using System.Globalization;
using System.Reflection;

namespace Paillave.Etl.TextFile;

public class FlatFileFieldDefinition
{
    public string[] TrueValues { get; internal set; } = null;
    public string[] FalseValues { get; internal set; } = null;
    public PropertyInfo PropertyInfo { get; set; }
    public int? Position { get; set; } = null;
    public CultureInfo CultureInfo { get; set; }
    public string ColumnName { get; set; } = null;
    public bool ForSourceName { get; internal set; } = false;
    public bool ForLineNumber { get; internal set; } = false;
    public bool ForRowGuid { get; internal set; }
}
