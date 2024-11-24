using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Paillave.Etl.XmlFile.Core
{
    public class XmlNodeParsed
    {
        public XmlNodeParsed(string sourceName, string nodeDefinitionName, string nodePath, Type type, object value, IDictionary<Type, Guid> correlationKeys)
        {
            SourceName = sourceName;
            NodeDefinitionName = nodeDefinitionName;
            NodePath = nodePath;
            Type = type;
            Value = value;
            CorrelationKeys = new ReadOnlyDictionary<Type, Guid>(correlationKeys);
        }
        public string SourceName { get; }
        public string NodeDefinitionName { get; }
        public string NodePath { get; }
        public Type Type { get; }
        public object Value { get; }
        public T GetValue<T>() => (T)Value;
        // public object[] ParentValues { get; internal set; }
        // public T GetValue<T>(int level = 0) => (T)(level == 0 ? Value : ParentValues[level - 1]);
        public ReadOnlyDictionary<Type, Guid> CorrelationKeys { get; }
    }
}
