using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Paillave.Etl.XmlFile.Core.Mapping;

namespace Paillave.Etl.XmlFile.Core;

public class XmlFileDefinition
{
    public Dictionary<string, string> PrefixToUriNameSpacesDictionary { get; } = new Dictionary<string, string>();
    internal List<IXmlNodeDefinition> XmlNodeDefinitions { get; } = new List<IXmlNodeDefinition>();

    public XmlFileDefinition AddNameSpace(string prefix, string uri)
    {
        this.PrefixToUriNameSpacesDictionary[prefix] = uri;
        return this;
    }

    public XmlFileDefinition AddNameSpaces(IDictionary<string, string> _prefixToUriNameSpacesDictionary)
    {
        foreach (var item in _prefixToUriNameSpacesDictionary)
            this.PrefixToUriNameSpacesDictionary[item.Key] = item.Value;
        return this;
    }

    public XmlFileDefinition AddNodeDefinition(IXmlNodeDefinition xmlNodeDefinition)
    {
        this.XmlNodeDefinitions.Add(xmlNodeDefinition);
        return this;
    }
    public XmlFileDefinition AddNodeDefinition<T>(string name, string nodeXPath, Expression<Func<IXmlFieldMapper, T>> expression)
    {
        this.XmlNodeDefinitions.Add(XmlNodeDefinition.Create<T>(name, nodeXPath, expression));
        return this;
    }
}
