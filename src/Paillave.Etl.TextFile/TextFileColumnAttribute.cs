using System;

namespace Paillave.Etl.TextFile
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TextFileColumnAttribute : Attribute
    {
        public string Name { get; set; }
        public string Format { get; set; }
        public int Index { get; set; }
        public int Width { get; set; }
    }
}
