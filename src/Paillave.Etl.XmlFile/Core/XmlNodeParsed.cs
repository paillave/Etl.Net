using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Paillave.Etl.XmlFile.Core;

public class XmlNodeParsed(string sourceName, string nodeDefinitionName, string nodePath, Type type, object value, IList<XmlNodeLevel> correlationKeys)
{
    public string SourceName { get; } = sourceName;
    public string NodeDefinitionName { get; } = nodeDefinitionName;
    public string NodePath { get; } = nodePath;
    public Type Type { get; } = type;
    public object Value { get; } = value;
    public T GetValue<T>() => (T)Value;
    // public object[] ParentValues { get; internal set; }
    // public T GetValue<T>(int level = 0) => (T)(level == 0 ? Value : ParentValues[level - 1]);
    public IReadOnlyDictionary<string, XmlNodeLevel> CorrelationKeys { get; } = new ReadOnlyDictionary<string, XmlNodeLevel>(correlationKeys.ToDictionary(ck => ck.Path));
}
