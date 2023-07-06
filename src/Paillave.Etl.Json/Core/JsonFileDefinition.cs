using System.Linq.Expressions;
using Paillave.Etl.Json.Core.Mapping;

namespace Paillave.Etl.Json.Core
{
    public class JsonFileDefinition
    {
        public Dictionary<string, string> PrefixToUriNameSpacesDictionary { get; } = new Dictionary<string, string>();
        internal List<IJsonNodeDefinition> JsonNodeDefinitions { get; } = new List<IJsonNodeDefinition>();

        public JsonFileDefinition AddNameSpace(string prefix, string uri)
        {
            this.PrefixToUriNameSpacesDictionary[prefix] = uri;
            return this;
        }

        public JsonFileDefinition AddNameSpaces(IDictionary<string, string> _prefixToUriNameSpacesDictionary)
        {
            foreach (var item in _prefixToUriNameSpacesDictionary)
                this.PrefixToUriNameSpacesDictionary[item.Key] = item.Value;
            return this;
        }

        public JsonFileDefinition AddNodeDefinition(IJsonNodeDefinition xmlNodeDefinition)
        {
            this.JsonNodeDefinitions.Add(xmlNodeDefinition);
            return this;
        }
        public JsonFileDefinition AddNodeDefinition<T>(string name, string nodeXPath, Expression<Func<IJsonFieldMapper, T>> expression)
        {
            this.JsonNodeDefinitions.Add(JsonNodeDefinition.Create<T>(name, nodeXPath, expression));
            return this;
        }
    }
}
