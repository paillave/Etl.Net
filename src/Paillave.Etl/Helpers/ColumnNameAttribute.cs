using System;

namespace Paillave.Etl.Helpers
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
