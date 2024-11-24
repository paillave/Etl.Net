using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Paillave.Etl.XmlFile.Core;

public class XmlNodeParsed
{
    public XmlNodeParsed(string sourceName, string nodeDefinitionName, string nodePath, Type type, object value, IList<XmlNodeLevel> correlationKeys)
    {
        SourceName = sourceName;
        NodeDefinitionName = nodeDefinitionName;
        NodePath = nodePath;
        Type = type;
        Value = value;
        CorrelationKeys = new ReadOnlyDictionary<string, XmlNodeLevel>(correlationKeys.ToDictionary(ck => ck.Path));
    }
    public string SourceName { get; }
    public string NodeDefinitionName { get; }
    public string NodePath { get; }
    public Type Type { get; }
    public object Value { get; }
    public T GetValue<T>() => (T)Value;
    // public object[] ParentValues { get; internal set; }
    // public T GetValue<T>(int level = 0) => (T)(level == 0 ? Value : ParentValues[level - 1]);
    public IReadOnlyDictionary<string, XmlNodeLevel> CorrelationKeys { get; }
}
