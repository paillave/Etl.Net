using System;

namespace Paillave.Etl.XmlFile.Core
{
    public class XmlNodeParsed
    {
        public string Name { get; internal set; }
        public string NodeXPath { get; internal set; }
        public Type Type { get; internal set; }
        public object Value { get; internal set; }
        public T GetValue<T>() => (T)Value;
    }
}
