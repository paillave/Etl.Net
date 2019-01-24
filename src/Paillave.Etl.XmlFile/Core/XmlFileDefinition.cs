using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Paillave.Etl.XmlFile.Core
{
    public class XmlFileDefinition
    {
        internal XmlNamespaceManager NameSpaceManager { get; } = new XmlNamespaceManager(new NameTable());
        internal List<IXmlNodeDefinition> XmlNodeDefinitions { get; } = new List<IXmlNodeDefinition>();

        public XmlFileDefinition AddNameSpace(string prefix, string uri)
        {
            this.NameSpaceManager.AddNamespace(prefix, uri);
            return this;
        }

        public XmlFileDefinition AddNameSpaces(IDictionary<string, string> prefixToUriNameSpacesDictionary)
        {
            if (prefixToUriNameSpacesDictionary != null)
                foreach (var item in prefixToUriNameSpacesDictionary)
                    this.NameSpaceManager.AddNamespace(item.Key, item.Value);
            return this;
        }

        public XmlFileDefinition AddNodeDefinition(IXmlNodeDefinition xmlNodeDefinition)
        {
            this.XmlNodeDefinitions.Add(xmlNodeDefinition);
            return this;
        }
    }
}
