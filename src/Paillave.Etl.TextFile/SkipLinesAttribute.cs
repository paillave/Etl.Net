using System;

namespace Paillave.Etl.TextFile
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SkipLinesAttribute : Attribute
    {
        public SkipLinesAttribute(int linesToSkip)
        {
            this.LinesToSkip = linesToSkip;

        }
        public int LinesToSkip { get; }
    }
}
