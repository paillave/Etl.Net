using System;

namespace Paillave.Etl.TextFile
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
