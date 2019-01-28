using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.XmlFile.Core.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Paillave.Etl.XmlFile.Core
{
    public class XmlObjectReader
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

        public XmlObjectReader(XmlFileDefinition xmlFileDefinition)
        {
            _xmlFileDefinition = xmlFileDefinition;
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
        private void ProcessEndOfAnyNode(Stack<string> nodes)
        {
            foreach (var item in _inScopeReadFields.Where(i => XmlReadFieldShouldBeCleanedUp(i, nodes.Count - 1)).ToList())
                _inScopeReadFields.Remove(item);
        }
        private void ProcessAttributeValue(Stack<string> nodes, string stringContent)
        {
            string key = $"/{string.Join("/", nodes.Reverse())}";
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
        private void ProcessEndOfNode(Stack<string> nodes, string text, Action<XmlNodeParsed> pushResult)
        {
            string key = $"/{string.Join("/", nodes.Reverse())}";
            if (_xmlFieldsDefinitionSearch.Contains(key))
            {
                ProcessAttributeValue(nodes, text);
            }
            else if (_xmlNodesDefinitionSearch.Contains(key))
            {
                var nd = _xmlFileDefinition.XmlNodeDefinitions.FirstOrDefault(i => i.NodePath == key);

                var objectBuilder = new ObjectBuilder(nd.Type);
                foreach (var inScopeReadField in _inScopeReadFields.Where(rf => rf.NodeDefinition.NodePath == key))
                    objectBuilder.Values[inScopeReadField.Definition.TargetPropertyInfo.Name] = inScopeReadField.Value;

                pushResult(new XmlNodeParsed
                {
                    NodeDefinitionName = nd.Name,
                    NodePath = nd.NodePath,
                    Type = nd.Type,
                    Value = objectBuilder.CreateInstance()
                });
            }
            ProcessEndOfAnyNode(nodes);
        }
        public void Read(Stream fileStream, Action<XmlNodeParsed> pushResult)
        {
            XmlReaderSettings xrs = new XmlReaderSettings();
            foreach (var item in _xmlFileDefinition.PrefixToUriNameSpacesDictionary)
                xrs.Schemas.Add(item.Key, item.Value);
            xrs.IgnoreWhitespace = true;
            xrs.IgnoreComments = true;
            xrs.IgnoreProcessingInstructions = true;

            var xmlReader = XmlReader.Create(fileStream, xrs);
            Stack<string> nodes = new Stack<string>();
            string lastTextValue = null;
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        bool isEmptyElement = xmlReader.IsEmptyElement;
                        lastTextValue = null;
                        nodes.Push(xmlReader.Name);
                        while (xmlReader.MoveToNextAttribute())
                        {
                            nodes.Push($"@{xmlReader.Name}");
                            ProcessAttributeValue(nodes, xmlReader.Value);
                            nodes.Pop();
                        }
                        if (isEmptyElement)
                        {
                            ProcessEndOfNode(nodes, null, pushResult);
                            nodes.Pop();
                        }
                        break;
                    case XmlNodeType.EndElement:
                        ProcessEndOfNode(nodes, lastTextValue, pushResult);
                        lastTextValue = null;
                        nodes.Pop();
                        break;
                    case XmlNodeType.Text:
                        lastTextValue = xmlReader.Value;
                        break;
                }
            }
        }
    }
}
