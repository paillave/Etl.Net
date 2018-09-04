using System;

namespace Paillave.Etl.TextFile
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnNameAttribute : Attribute
    {
        public string Name { get; }
        public ColumnNameAttribute(string name)
        {
            this.Name = name;
        }
    }
}
