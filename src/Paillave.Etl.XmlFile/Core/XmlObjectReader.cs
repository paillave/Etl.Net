using Paillave.Etl.Core;
using Paillave.Etl.XmlFile.Core.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Paillave.Etl.XmlFile.Core;
[Obsolete]
public class XmlObjectReader : IXmlObjectReader
{
    private class XmlReadField
    {
        public XmlFieldDefinition Definition { get; set; }
        public IXmlNodeDefinition NodeDefinition { get; set; }
        public int Depth { get; set; }
        public object Value { get; set; }
    }

    private HashSet<string> _xmlFieldsDefinitionSearch;
    private HashSet<string> _xmlNodesDefinitionSearch;

    private readonly List<XmlReadField> _inScopeReadFields = new List<XmlReadField>();
    private readonly XmlFileDefinition _xmlFileDefinition;
    private readonly string _sourceName;
    private readonly Action<XmlNodeParsed> _pushResult;

    public XmlObjectReader(XmlFileDefinition xmlFileDefinition, string sourceName, Action<XmlNodeParsed> pushResult)
    {
        _xmlFileDefinition = xmlFileDefinition;
        this._sourceName = sourceName;
        this._pushResult = pushResult;
        _xmlNodesDefinitionSearch = new HashSet<string>(xmlFileDefinition.XmlNodeDefinitions.Select(i => i.NodePath).Distinct());
        _xmlFieldsDefinitionSearch = new HashSet<string>(xmlFileDefinition.XmlNodeDefinitions.SelectMany(nd => nd.GetXmlFieldDefinitions().Select(fd => fd.NodePath)).Distinct());
    }
    private bool XmlReadFieldShouldBeCleanedUp(XmlReadField xmlReadField, int depth)
    {
        var depthScope = xmlReadField.Definition.DepthScope;
        int depthLimit;
        if (depthScope > 0)
            depthLimit = depthScope;
        else
            depthLimit = xmlReadField.Depth + depthScope;
        return depth < depthLimit;
    }
    private void ProcessEndOfAnyNode(Stack<NodeLevel> nodes)
    {
        foreach (var item in _inScopeReadFields.Where(i => XmlReadFieldShouldBeCleanedUp(i, nodes.Count - 1)).ToList())
            _inScopeReadFields.Remove(item);
    }
    private void ProcessAttributeValue(string key, Stack<NodeLevel> nodes, string stringContent)
    {
        // string key = $"/{string.Join("/", nodes.Reverse())}";
        if (!_xmlFieldsDefinitionSearch.Contains(key)) return;
        var fds = _xmlFileDefinition.XmlNodeDefinitions.SelectMany(nd => nd.GetXmlFieldDefinitions().Select(fd => new { Fd = fd, Nd = nd })).Where(i => i.Fd.NodePath == key).ToList();
        if (string.IsNullOrWhiteSpace(stringContent))
        {
            foreach (var fd in fds)
            {
                _inScopeReadFields.Add(new XmlReadField
                {
                    Depth = nodes.Count - 1,
                    Definition = fd.Fd,
                    NodeDefinition = fd.Nd,
                    Value = null
                });
            }
        }
        else
        {
            foreach (var fd in fds)
            {
                _inScopeReadFields.Add(new XmlReadField
                {
                    Depth = nodes.Count - 1,
                    Definition = fd.Fd,
                    NodeDefinition = fd.Nd,
                    Value = fd.Fd.Convert(stringContent)
                });
            }
        }
    }
    private string ComputeKey(Stack<NodeLevel> nodes) => $"/{string.Join("/", nodes.Select(i => i.Name).Reverse())}";
    private void ProcessEndOfNode(Stack<NodeLevel> nodes, string text, Action<XmlNodeParsed> pushResult, string sourceName)
    {
        string key = ComputeKey(nodes);
        if (_xmlFieldsDefinitionSearch.Contains(key))
        {
            ProcessAttributeValue(key, nodes, text);
        }
        else if (_xmlNodesDefinitionSearch.Contains(key))
        {
            var (value, nd) = CreateValue(sourceName, key);
            pushResult(new XmlNodeParsed(sourceName, nd.Name, nd.NodePath, nd.Type, value, new Dictionary<Type, Guid>()));
        }
        ProcessEndOfAnyNode(nodes);
    }

    private (object value, IXmlNodeDefinition nd) CreateValue(string sourceName, string key)
    {
        var nd = _xmlFileDefinition.XmlNodeDefinitions.FirstOrDefault(i => i.NodePath == key);
        var objectBuilder = new ObjectBuilder(nd.Type);
        foreach (var inScopeReadField in _inScopeReadFields.Where(rf => rf.NodeDefinition.NodePath == key))
            objectBuilder.Values[inScopeReadField.Definition.TargetPropertyInfo.Name] = inScopeReadField.Value;
        foreach (var propName in nd.GetXmlFieldDefinitions().Where(i => i.ForRowGuid).Select(i => i.TargetPropertyInfo.Name).ToList())
            objectBuilder.Values[propName] = Guid.NewGuid();
        foreach (var propName in nd.GetXmlFieldDefinitions().Where(i => i.ForSourceName).Select(i => i.TargetPropertyInfo.Name).ToList())
            objectBuilder.Values[propName] = sourceName;
        return (objectBuilder.CreateInstance(), nd);
    }

    public void Read(Stream fileStream, CancellationToken cancellationToken)
    {
        XmlReaderSettings xrs = new XmlReaderSettings();
        foreach (var item in _xmlFileDefinition.PrefixToUriNameSpacesDictionary)
            xrs.Schemas.Add(item.Key, item.Value);
        xrs.IgnoreWhitespace = true;
        xrs.IgnoreComments = true;
        xrs.IgnoreProcessingInstructions = true;

        var xmlReader = XmlReader.Create(fileStream, xrs);
        Stack<NodeLevel> nodes = new Stack<NodeLevel>();
        string lastTextValue = null;
        while (xmlReader.Read())
        {
            if (cancellationToken.IsCancellationRequested) break;
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    bool isEmptyElement = xmlReader.IsEmptyElement;
                    lastTextValue = null;
                    nodes.Push(new NodeLevel { Name = xmlReader.Name, Guid = Guid.NewGuid() });
                    while (xmlReader.MoveToNextAttribute())
                    {
                        nodes.Push(new NodeLevel { Name = $"@{xmlReader.Name}", Guid = null });
                        ProcessAttributeValue(ComputeKey(nodes), nodes, xmlReader.Value);
                        nodes.Pop();
                    }
                    if (isEmptyElement)
                    {
                        ProcessEndOfNode(nodes, null, _pushResult, _sourceName);
                        nodes.Pop();
                    }
                    break;
                case XmlNodeType.EndElement:
                    ProcessEndOfNode(nodes, lastTextValue, _pushResult, _sourceName);
                    lastTextValue = null;
                    nodes.Pop();
                    break;
                case XmlNodeType.Text:
                    lastTextValue = xmlReader.Value;
                    break;
            }
        }
    }
    private struct NodeLevel
    {
        public string Name { get; set; }
        public Guid? Guid { get; set; }
    }
}
