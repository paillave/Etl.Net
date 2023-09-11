using System;
using System.Collections.Generic;

namespace Paillave.Etl.XmlFile.Core
{
    public class XmlNodeParsed
    {
        public string SourceName { get; internal set; }
        public string NodeDefinitionName { get; internal set; }
        public string NodePath { get; internal set; }
        public Type Type { get; internal set; }
        public object Value { get; internal set; }
        public T GetValue<T>() => (T)Value;
        // public object[] ParentValues { get; internal set; }
        // public T GetValue<T>(int level = 0) => (T)(level == 0 ? Value : ParentValues[level - 1]);
        public HashSet<Guid> CorrelationKeys { get; set; } = new HashSet<Guid>();
    }
}
