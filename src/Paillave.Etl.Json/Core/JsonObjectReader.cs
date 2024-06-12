using Paillave.Etl.Core;
using Paillave.Etl.Json.Core.Mapping;
using System.Buffers;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Xml;

namespace Paillave.Etl.Json.Core
{
    public class JsonObjectReader
    {
        private class JsonReadField
        {
            public JsonFieldDefinition Definition { get; set; }
            public IJsonNodeDefinition NodeDefinition { get; set; }
            public int Depth { get; set; }
            public object Value { get; set; }
        }

        private HashSet<string> _jsonFieldsDefinitionSearch;
        private HashSet<string> _jsonNodesDefinitionSearch;

        private readonly List<JsonReadField> _inScopeReadFields = new List<JsonReadField>();
        private readonly JsonFileDefinition _jsonFileDefinition;

        public JsonObjectReader(JsonFileDefinition jsonFileDefinition)
        {
            _jsonFileDefinition = jsonFileDefinition;
            _jsonNodesDefinitionSearch = new HashSet<string>(jsonFileDefinition.JsonNodeDefinitions.Select(i => i.NodePath).Distinct());
            _jsonFieldsDefinitionSearch = new HashSet<string>(jsonFileDefinition.JsonNodeDefinitions.SelectMany(nd => nd.GetJsonFieldDefinitions().Select(fd => fd.NodePath)).Distinct());
        }
        private bool JsonReadFieldShouldBeCleanedUp(JsonReadField jsonReadField, int depth)
        {
            var depthScope = jsonReadField.Definition.DepthScope;
            int depthLimit;
            if (depthScope > 0)
                depthLimit = depthScope;
            else
                depthLimit = jsonReadField.Depth + depthScope;
            return depth < depthLimit;
        }
        private void ProcessEndOfAnyNode(Stack<NodeLevel> nodes)
        {
            foreach (var item in _inScopeReadFields.Where(i => JsonReadFieldShouldBeCleanedUp(i, nodes.Count - 1)).ToList())
                _inScopeReadFields.Remove(item);
        }
        private void ProcessAttributeValue(string key, Stack<NodeLevel> nodes, string stringContent)
        {
            // string key = $"/{string.Join("/", nodes.Reverse())}";
            if (!_jsonFieldsDefinitionSearch.Contains(key)) return;
            var fds = _jsonFileDefinition.JsonNodeDefinitions.SelectMany(nd => nd.GetJsonFieldDefinitions().Select(fd => new { Fd = fd, Nd = nd })).Where(i => i.Fd.NodePath == key).ToList();
            if (string.IsNullOrWhiteSpace(stringContent))
            {
                foreach (var fd in fds)
                {
                    _inScopeReadFields.Add(new JsonReadField
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
                    _inScopeReadFields.Add(new JsonReadField
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
        private void ProcessEndOfNode(Stack<NodeLevel> nodes, string text, Action<JsonNodeParsed> pushResult, string sourceName)
        {
            string key = ComputeKey(nodes);
            if (_jsonFieldsDefinitionSearch.Contains(key))
            {
                ProcessAttributeValue(key, nodes, text);
            }
            else if (_jsonNodesDefinitionSearch.Contains(key))
            {
                var (value, nd) = CreateValue(sourceName, key);
                pushResult(new JsonNodeParsed
                {
                    NodeDefinitionName = nd.Name,
                    SourceName = sourceName,
                    NodePath = nd.NodePath,
                    Type = nd.Type,
                    Value = value,
                    CorrelationKeys = nodes.Select(i => i.Guid).Where(i => i.HasValue).Select(i => i.Value).ToHashSet()
                });
            }
            ProcessEndOfAnyNode(nodes);
        }

        private (object value, IJsonNodeDefinition nd) CreateValue(string sourceName, string key)
        {
            var nd = _jsonFileDefinition.JsonNodeDefinitions.FirstOrDefault(i => i.NodePath == key);
            var objectBuilder = new ObjectBuilder(nd.Type);
            foreach (var inScopeReadField in _inScopeReadFields.Where(rf => rf.NodeDefinition.NodePath == key))
                objectBuilder.Values[inScopeReadField.Definition.TargetPropertyInfo.Name] = inScopeReadField.Value;
            foreach (var propName in nd.GetJsonFieldDefinitions().Where(i => i.ForRowGuid).Select(i => i.TargetPropertyInfo.Name).ToList())
                objectBuilder.Values[propName] = Guid.NewGuid();
            foreach (var propName in nd.GetJsonFieldDefinitions().Where(i => i.ForSourceName).Select(i => i.TargetPropertyInfo.Name).ToList())
                objectBuilder.Values[propName] = sourceName;
            return (objectBuilder.CreateInstance(), nd);
        }

        public void Read(Stream fileStream, string sourceName, Action<JsonNodeParsed> pushResult, CancellationToken cancellationToken)
        {
            var readerSettings = new JsonReaderOptions();
            readerSettings.CommentHandling = JsonCommentHandling.Skip;

            using var streamReader = new StreamReader(fileStream);
            var jsonReader = new Utf8JsonReader(new ReadOnlySequence<byte>(streamReader.ReadToEnd().Select(Convert.ToByte).ToArray()), readerSettings);
            
            var nodes = new Stack<NodeLevel>();
            string lastPropertyName = null;
            string lastArrayPropertyName = null;
            while (jsonReader.Read())
            {
                if (cancellationToken.IsCancellationRequested) break;
                switch (jsonReader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        lastPropertyName = jsonReader.GetString();
                        break;

                    case JsonTokenType.EndObject:
                        if (nodes.Any())
                        { 
                            ProcessEndOfNode(nodes, null, pushResult, sourceName);
                            nodes.Pop();
                        }
                        break;

                    case JsonTokenType.StartArray:
                        lastArrayPropertyName = lastPropertyName;
                        break;

                    case JsonTokenType.EndArray:
                        lastArrayPropertyName = null;
                        break;

                    case JsonTokenType.StartObject:
                        if (!string.IsNullOrEmpty(lastPropertyName))
                        {
                            nodes.Push(new NodeLevel { Name = lastArrayPropertyName ?? lastPropertyName, Guid = Guid.NewGuid() });
                        }
                        break;

                    // read values into last-value var
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        nodes.Push(new NodeLevel { Name = lastPropertyName, Guid = Guid.NewGuid() });
                        ProcessEndOfNode(nodes, jsonReader.GetString(), pushResult, sourceName);
                        nodes.Pop();
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
}
