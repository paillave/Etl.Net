using System;

namespace Paillave.Etl.TextFile
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ColumnDelimitedAttribute : Attribute
    {
        public ColumnDelimitedAttribute(char separator, char textIdentifier)
        {
            this.Separator = separator;
            this.TextIdentifier = textIdentifier;

        }
        public char Separator { get; }
        public char TextIdentifier { get; }
    }
}
