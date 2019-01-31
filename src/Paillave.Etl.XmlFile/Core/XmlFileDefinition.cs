using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Paillave.Etl.XmlFile.Core
{
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
    }
}
