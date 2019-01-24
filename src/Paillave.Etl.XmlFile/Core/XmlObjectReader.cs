using GotDotNet.XPath;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.XmlFile.Core.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Paillave.Etl.XmlFile.Core
{
    public class XmlObjectReader
    {
        private XmlFileDefinition _xmlFileDefinition;
        private XPathReader _xpathReader;
        private XPathCollection _xPathCollection;
        private class NodeDefinitionState
        {
            public IXmlNodeDefinition XmlNodeDefinition { get; }
            private IXmlNamespaceResolver _namespaceResolver;
            private ObjectBuilder _objectBuilder;
            private Dictionary<int, XmlFieldDefinition> _xmlFieldDefinitions = new Dictionary<int, XmlFieldDefinition>();
            public NodeDefinitionState(IXmlNodeDefinition xmlNodeDefinition, IXmlNamespaceResolver namespaceResolver)
            {
                _objectBuilder = new ObjectBuilder(xmlNodeDefinition.Type);
                _namespaceResolver = namespaceResolver;
                XmlNodeDefinition = xmlNodeDefinition;
            }
            public void AddFieldDefinition(int id, XmlFieldDefinition xmlFieldDefinition)
            {
                _xmlFieldDefinitions[id] = xmlFieldDefinition;
            }
            public void SetValue(XPathReader xpathReader)
            {
                var xmlFieldDefinition = _xmlFieldDefinitions.FirstOrDefault(i => xpathReader.Match(i.Key)).Value;
                if (xmlFieldDefinition == null) return;
                object val;
                if (xpathReader.NodeType == XmlNodeType.Element)
                    val = xpathReader.ReadElementContentAs(xmlFieldDefinition.TargetPropertyInfo.PropertyType, _namespaceResolver);//.ReadString();
                else
                    val = xpathReader.ReadContentAs(xmlFieldDefinition.TargetPropertyInfo.PropertyType, _namespaceResolver); //Value
                _objectBuilder.Values[xmlFieldDefinition.TargetPropertyInfo.Name] = val;
            }
            public object CreateInstance()
            {
                var ret = _objectBuilder.CreateInstance();
                _objectBuilder.Values = new Dictionary<string, object>();
                return ret;
            }
        }
        private IDictionary<int, NodeDefinitionState> _nodeDefinitionStates = new Dictionary<int, NodeDefinitionState>();
        public XmlObjectReader(XmlReader xmlReader, XmlFileDefinition xmlFileDefinition, Action<XmlNodeParsed> pushResult)
        {
            _pushResult = pushResult;
            _xmlFileDefinition = xmlFileDefinition;

            _xPathCollection = new XPathCollection(_xmlFileDefinition.NameSpaceManager);

            foreach (var xmlNodeDefinition in xmlFileDefinition.XmlNodeDefinitions)
            {
                var nodeDefinitionId = _xPathCollection.Add(xmlNodeDefinition.NodeXPath);
                var nodeDefinitionState = new NodeDefinitionState(xmlNodeDefinition, _xmlFileDefinition.NameSpaceManager);
                foreach (var xmlFieldDefinition in xmlNodeDefinition.GetXmlFieldDefinitions())
                {
                    var xmlFieldDefinitionId = _xPathCollection.Add(xmlFieldDefinition.XPathQuery);
                    nodeDefinitionState.AddFieldDefinition(xmlFieldDefinitionId, xmlFieldDefinition);
                }
                _nodeDefinitionStates[nodeDefinitionId] = nodeDefinitionState;
            }
            _xpathReader = new XPathReader(xmlReader, _xPathCollection);
        }
        private NodeDefinitionState _currentNodeDefinitionState = null;
        private Action<XmlNodeParsed> _pushResult;
        private bool TryToDefineAsCurrentNodeDefinitionState()
        {
            if (_xpathReader.NodeType == XmlNodeType.EndElement && _currentNodeDefinitionState != null)
            {
                _pushResult(new XmlNodeParsed
                {
                    Name = _currentNodeDefinitionState.XmlNodeDefinition.Name,
                    NodeXPath = _currentNodeDefinitionState.XmlNodeDefinition.NodeXPath,
                    Type = _currentNodeDefinitionState.XmlNodeDefinition.Type,
                    Value = _currentNodeDefinitionState.CreateInstance()
                });
                _currentNodeDefinitionState = null;
                return true;
            }
            if (_xpathReader.NodeType != XmlNodeType.Element) return false;
            var state = _nodeDefinitionStates.FirstOrDefault(i => _xpathReader.Match(i.Key)).Value;
            if (state == null) return false;
            if (_currentNodeDefinitionState != null)
            {
                _pushResult(new XmlNodeParsed
                {
                    Name = _currentNodeDefinitionState.XmlNodeDefinition.Name,
                    NodeXPath = _currentNodeDefinitionState.XmlNodeDefinition.NodeXPath,
                    Type = _currentNodeDefinitionState.XmlNodeDefinition.Type,
                    Value = _currentNodeDefinitionState.CreateInstance()
                });
            }
            _currentNodeDefinitionState = state;
            return true;
        }
        public void Read()
        {
            while (_xpathReader.ReadUntilMatch())
                if (!TryToDefineAsCurrentNodeDefinitionState())
                    if (_currentNodeDefinitionState != null)
                        _currentNodeDefinitionState.SetValue(_xpathReader);
        }
    }
}
