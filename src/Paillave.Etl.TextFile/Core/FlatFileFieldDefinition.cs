using System.Globalization;
using System.Reflection;

namespace Paillave.Etl.TextFile.Core
{
    public class FlatFileFieldDefinition
    {
        public PropertyInfo PropertyInfo { get; set; }
        public int? Position { get; set; } = null;
        public CultureInfo CultureInfo { get; set; }
        public string ColumnName { get; set; } = null;
    }
}
