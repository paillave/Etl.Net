using System.Globalization;
using System.Reflection;

namespace Paillave.Etl.TextFile.Core
{
    public class FieldDefinition
    {
        public PropertyInfo PropertyInfo { get; set; }
        public int? Position { get; set; } = null;
        public CultureInfo CultureInfo { get; set; }
        public string Name { get; set; } = null;
    }
}
