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
        private class XmlReadField
        {
            public XmlFieldToRead FieldToRead { get; set; }
            public int Depth { get; set; }
            public object Value { get; set; }
        }
        private class XmlReadNode
        {
            public XmlNodeToRead NodeToRead { get; set; }
            public int Depth { get; set; }
        }
        private class XmlNodeToRead
        {
            public int SearchId { get; set; }
            public IXmlNodeDefinition NodeDefinition { get; set; }
            public List<XmlFieldToRead> FieldsToRead { get; set; }
        }
        private class XmlFieldToRead
        {
            public int SearchId { get; set; }
            public XmlFieldDefinition FieldDefinition { get; set; }
        }
        private readonly List<XmlReadField> _inScopeReadFields = new List<XmlReadField>();
        private readonly List<XmlNodeToRead> _nodesToRead;

        private XmlFileDefinition _xmlFileDefinition;
        private XPathReader _xpathReader;
        private XPathCollection _xPathCollection;
        public XmlObjectReader(XmlReader xmlReader, XmlFileDefinition xmlFileDefinition, Action<XmlNodeParsed> pushResult)
        {
            _pushResult = pushResult;
            _xmlFileDefinition = xmlFileDefinition;

            _xPathCollection = new XPathCollection(_xmlFileDefinition.NameSpaceManager);

            _nodesToRead = xmlFileDefinition.XmlNodeDefinitions.Select(xmlNodeDefinition =>
            {
                var nodeToReadSearchId = _xPathCollection.Add(xmlNodeDefinition.NodeXPath);
                return new XmlNodeToRead
                {
                    NodeDefinition = xmlNodeDefinition,
                    SearchId = nodeToReadSearchId,
                    FieldsToRead = xmlNodeDefinition.GetXmlFieldDefinitions().Select(xmlFieldDefinition =>
                    {
                        var fieldToReadSearchId = _xPathCollection.Add(xmlFieldDefinition.XPathQuery);
                        return new XmlFieldToRead
                        {
                            SearchId = fieldToReadSearchId,
                            FieldDefinition = xmlFieldDefinition
                        };
                    }).ToList()
                };
            }).ToList();

            _xpathReader = new XPathReader(xmlReader, _xPathCollection);
        }
        private Action<XmlNodeParsed> _pushResult;
        private bool XmlReadFieldShouldBeCleanedUp(XmlReadField xmlReadField)
        {
            var depthScope = xmlReadField.FieldToRead.FieldDefinition.DepthScope;
            int depthLimit;
            if (depthScope > 0)
                depthLimit = depthScope;
            else
                depthLimit = xmlReadField.Depth + depthScope;
            return _xpathReader.Depth < depthLimit;
        }
        private void ProcessEndOfAnyNode()
        {
            foreach (var item in _inScopeReadFields.Where(XmlReadFieldShouldBeCleanedUp).ToList())
                _inScopeReadFields.Remove(item);
        }
        private void ProcessEndOfSearchedNode()
        {
            if (_xpathReader.NodeType == XmlNodeType.EndElement)
            {
                var nodeToRead = _nodesToRead.FirstOrDefault(i => _xpathReader.Match(i.SearchId));
                if (nodeToRead != null)
                {
                    var objectBuilder = new ObjectBuilder(nodeToRead.NodeDefinition.Type);
                    foreach (var inScopeReadField in _inScopeReadFields.Where(i => nodeToRead.FieldsToRead.Any(j => j.SearchId == i.FieldToRead.SearchId)))
                        objectBuilder.Values[inScopeReadField.FieldToRead.FieldDefinition.TargetPropertyInfo.Name] = inScopeReadField.Value;

                    _pushResult(new XmlNodeParsed
                    {
                        Name = nodeToRead.NodeDefinition.Name,
                        NodeXPath = nodeToRead.NodeDefinition.NodeXPath,
                        Type = nodeToRead.NodeDefinition.Type,
                        Value = objectBuilder.CreateInstance()
                    });
                }
                ProcessEndOfAnyNode();
            }
        }
        private object GetFieldValue(XmlFieldDefinition xmlFieldDefinition)
        {
            if (_xpathReader.NodeType == XmlNodeType.Element)
                return _xpathReader.ReadElementContentAs(xmlFieldDefinition.TargetPropertyInfo.PropertyType, _xmlFileDefinition.NameSpaceManager);//.ReadString();
            else
                return _xpathReader.ReadContentAs(xmlFieldDefinition.TargetPropertyInfo.PropertyType, _xmlFileDefinition.NameSpaceManager); //Value
        }
        private void ProcessSearchedField()
        {
            var matchingFieldsToReads = _nodesToRead.SelectMany(nodeToRead => nodeToRead.FieldsToRead.Where(fieldToRead => _xpathReader.Match(fieldToRead.SearchId))).ToList();

            foreach (var matchingFieldToRead in matchingFieldsToReads)
            {
                var inScopeReadField = _inScopeReadFields.FirstOrDefault(i => i.Depth == _xpathReader.Depth && i.FieldToRead.SearchId == matchingFieldToRead.SearchId);
                if (inScopeReadField == null)
                {
                    inScopeReadField = new XmlReadField
                    {
                        Depth = _xpathReader.Depth,
                        FieldToRead = matchingFieldToRead,
                        Value = GetFieldValue(matchingFieldToRead.FieldDefinition)
                    };
                    _inScopeReadFields.Add(inScopeReadField);
                }
                else
                {
                    inScopeReadField.Value = GetFieldValue(matchingFieldToRead.FieldDefinition);
                }
            }
        }
        public void Read()
        {
            while (_xpathReader.ReadUntilMatch(ProcessEndOfAnyNode))
            {
                ProcessEndOfSearchedNode();
                ProcessSearchedField();
            }
        }
    }
}
