using System;

namespace Paillave.Etl.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnFormatAttribute : Attribute
    {
        public ColumnFormatAttribute(string format)
        {
            this.Format = format;
        }
        public string Format { get; }
    }
}
