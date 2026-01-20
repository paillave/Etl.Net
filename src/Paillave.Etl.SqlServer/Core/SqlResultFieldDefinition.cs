using System.Globalization;
using System.Reflection;

namespace Paillave.Etl.SqlServer.Core;

public class SqlResultFieldDefinition
{
    public PropertyInfo PropertyInfo { get; set; }
    public string ColumnName { get; set; } = null;
}
