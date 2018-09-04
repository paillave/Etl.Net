using System;

namespace Paillave.Etl.TextFile
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnWidthAttribute : Attribute
    {
        public int Width { get; }
        public ColumnWidthAttribute(int width)
        {
            this.Width = width;
        }
    }
}
