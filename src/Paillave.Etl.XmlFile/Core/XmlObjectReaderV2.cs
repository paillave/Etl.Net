using Paillave.Etl.Core;
using Paillave.Etl.XmlFile.Core.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Paillave.Etl.XmlFile.Core;
public class XmlObjectReaderV2(XmlFileDefinition xmlFileDefinition, string sourceName, Action<XmlNodeParsed> pushResult) : IXmlObjectReader
{
    private readonly NodePropertyBags _nodePropertyBags = new(sourceName, xmlFileDefinition, pushResult);
    private readonly XmlFileDefinition _xmlFileDefinition = xmlFileDefinition;

    private class XmlPath
    {
        private readonly Stack<XmlNodeLevel> _nodes = new();
        private string? _attribute = null;
        public void UnStackAttribute() => _attribute = null;
        public void StackAttribute(string attribute) => _attribute = attribute;
        public void StackNode(string node) => _nodes.Push(new XmlNodeLevel(node, Guid.NewGuid(), $"{GetPath()}/{node}"));
        public void UnStackNode() => _nodes.Pop();
        public string GetPath() => $"/{string.Join("/", _nodes.Select((i) => i.Node).Reverse())}{(_attribute == null ? "" : $"/@{_attribute}")}";
        public IList<XmlNodeLevel> GetCorrelationKeys() => _nodes.Reverse().ToList();
        public override string ToString() => GetPath();
    }
    private class NodePropertyBags(string sourceName, XmlFileDefinition xmlFileDefinition, Action<XmlNodeParsed> pushResult)
    {
        private readonly Dictionary<string, PropertyBag> _propertyBags = xmlFileDefinition.XmlNodeDefinitions.ToDictionary(i => i.NodePath, i => new PropertyBag(sourceName, i));
        private readonly Action<XmlNodeParsed> _pushResult = pushResult;

        public void SetValue(string key, string? value)
        {
            foreach (var propertyBag in _propertyBags)
                propertyBag.Value.SetValue(key, value);
        }
        public void StartNewNode(string key)
        {
            if (_propertyBags.TryGetValue(key, out var propertyBag))
                propertyBag.ResetValues();
        }
        public void EndNode(XmlPath xmlPath)
        {
            if (_propertyBags.TryGetValue(xmlPath.ToString(), out var propertyBag))
            {
                var value = propertyBag.CreateRow();
                _pushResult(new XmlNodeParsed(
                    propertyBag.SourceName,
                    propertyBag.XmlNodeDefinition.Name,
                    propertyBag.XmlNodeDefinition.NodePath,
                    propertyBag.XmlNodeDefinition.Type,
                    value,
                    xmlPath.GetCorrelationKeys()));
            }
        }
    }
    private class PropertyBag
    {
        public string SourceName { get; }
        public IXmlNodeDefinition XmlNodeDefinition { get; }
        private readonly List<XmlFieldDefinition> _xmlFieldDefinitions;
        private readonly HashSet<string> _valuesPath;
        private readonly Dictionary<string, string> _xmlValues = new();
        public PropertyBag(string sourceName, IXmlNodeDefinition xmlNodeDefinition)
        {
            SourceName = sourceName;
            this.XmlNodeDefinition = xmlNodeDefinition;
            _xmlFieldDefinitions = xmlNodeDefinition.GetXmlFieldDefinitions().ToList();
            this._valuesPath = _xmlFieldDefinitions.Select(i => i.NodePath).ToHashSet();
        }
        public void SetValue(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
                //   ^(?<segment>/(?<segmentName>[^/[]+)([[](?<segmentFilter>(?<segmentFilterAttribute>[^]=]+)=""(?<segmentFilterValue>[^""]*)"")[]])?)+$
                //   /zer[sdfsdfsd="er"]/wfxf/trtr[qdff="bg"]
                //   https://regex101.com/r/tG1jF6/1
// XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            if (_valuesPath.Contains(key))
                _xmlValues[key] = value;
        }

        // public void SetValue(string key, string? value, Dictionary<string, string> xmlAttributes)
        // {
        //     if (string.IsNullOrWhiteSpace(value))
        //         return;
        //     if (TryGetPathMatch(key, xmlAttributes, out var pathMatch) && pathMatch != null)
        //         _xmlValues[pathMatch] = value;
        // }
        // private bool TryGetPathMatch(string key, Dictionary<string, string> xmlAttributes, out string? pathMatch)
        // {
        //     pathMatch = null;
        //     if (_valuesPath.Contains(key))
        //     {
        //         pathMatch = key;
        //         return true;
        //     }
        //     return false;
        // }

        public object CreateRow()
        {
            var objectBuilder = new ObjectBuilder(XmlNodeDefinition.Type);
            var matchingProperties = _xmlFieldDefinitions.Join(_xmlValues, i => i.NodePath, i => i.Key, (xmlFieldDefinition, xmlFieldDefinitionValue) => new { xmlFieldDefinition, xmlFieldDefinitionValue }).ToList();
            foreach (var matchingProperty in matchingProperties)
                objectBuilder.Values[matchingProperty.xmlFieldDefinition.TargetPropertyInfo.Name] = matchingProperty.xmlFieldDefinition.Convert(matchingProperty.xmlFieldDefinitionValue.Value);
            foreach (var propName in _xmlFieldDefinitions.Where(i => i.ForRowGuid).Select(i => i.TargetPropertyInfo.Name).ToList())
                objectBuilder.Values[propName] = Guid.NewGuid();
            foreach (var propName in _xmlFieldDefinitions.Where(i => i.ForSourceName).Select(i => i.TargetPropertyInfo.Name).ToList())
                objectBuilder.Values[propName] = SourceName;
            return objectBuilder.CreateInstance();
        }
        public void ResetValues()
        {
            foreach (var item in _xmlValues.Where(i => i.Key.StartsWith(XmlNodeDefinition.NodePath)).ToList())
                _xmlValues.Remove(item.Key);
        }
    }

    public void Read(Stream fileStream, CancellationToken cancellationToken)
    {
        XmlReaderSettings xrs = new();
        foreach (var item in _xmlFileDefinition.PrefixToUriNameSpacesDictionary)
            xrs.Schemas.Add(item.Key, item.Value);
        xrs.IgnoreWhitespace = true;
        xrs.IgnoreComments = true;
        xrs.IgnoreProcessingInstructions = true;

        var xmlPath = new XmlPath();

        var xmlReader = XmlReader.Create(fileStream, xrs);
        string? lastTextValue = null;
        while (xmlReader.Read())
        {
            if (cancellationToken.IsCancellationRequested) break;
            switch (xmlReader.NodeType)
            {
                case XmlNodeType.Element:
                    lastTextValue = null;
                    bool isEmptyElement = xmlReader.IsEmptyElement;
                    xmlPath.StackNode(xmlReader.Name);
                    _nodePropertyBags.StartNewNode(xmlPath.ToString());
                    while (xmlReader.MoveToNextAttribute())
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        xmlPath.StackAttribute(xmlReader.Name);
                        _nodePropertyBags.SetValue(xmlPath.ToString(), xmlReader.Value);
                        xmlPath.UnStackAttribute();
                    }
                    if (isEmptyElement)
                    {
                        _nodePropertyBags.EndNode(xmlPath);
                        xmlPath.UnStackNode();
                    }
                    break;
                case XmlNodeType.EndElement:
                    _nodePropertyBags.SetValue(xmlPath.ToString(), lastTextValue);
                    _nodePropertyBags.EndNode(xmlPath);
                    lastTextValue = null;
                    xmlPath.UnStackNode();
                    break;
                case XmlNodeType.Text:
                    lastTextValue = xmlReader.Value;
                    break;
            }
        }
    }
}
public readonly struct XmlNodeLevel
{
    public XmlNodeLevel(string node, Guid correlationId, string path)
        => (Node, CorrelationId, Path) = (node, correlationId, path);
    public string Node { get; }
    public Guid CorrelationId { get; }
    public string Path { get; }
}
