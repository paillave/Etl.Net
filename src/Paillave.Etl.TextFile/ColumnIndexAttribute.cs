using System;

namespace Paillave.Etl.TextFile
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnIndexAttribute : Attribute
    {
        public int Index { get; }
        public ColumnIndexAttribute(int index)
        {
            this.Index = index;
        }
    }
}
