using System.Globalization;
using System.Reflection;

namespace Paillave.Etl.ExcelFile.Core;

public class ExcelFileFieldDefinition
{
    public PropertyInfo PropertyInfo { get; set; }
    public int? Position { get; set; } = null;
    public CultureInfo CultureInfo { get; set; }
    public string ColumnName { get; set; } = null;
}
