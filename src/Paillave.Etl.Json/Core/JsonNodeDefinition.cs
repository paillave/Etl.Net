using Paillave.Etl.Json.Core.Mapping;
using Paillave.Etl.Json.Core.Mapping.Visitors;
using System.Linq.Expressions;

namespace Paillave.Etl.Json.Core
{
    public static class JsonNodeDefinition
    {
        public static JsonNodeDefinition<T> Create<T>(string name, string nodePath, Expression<Func<IJsonFieldMapper, T>> expression)
            => new JsonNodeDefinition<T>(name, nodePath).WithMap(expression);
    }
    public class JsonNodeDefinition<T> : IJsonNodeDefinition
    {
        public string Name { get; set; }
        public IList<JsonFieldDefinition> _jsonFieldDefinitions = new List<JsonFieldDefinition>();

        public IList<JsonFieldDefinition> GetJsonFieldDefinitions() => _jsonFieldDefinitions.ToList();
        public string NodePath { get; private set; }

        public Type Type { get; } = typeof(T);

        public JsonNodeDefinition(string name, string nodePath)
        {
            this.Name = name;
            this.NodePath = nodePath;
        }
        public JsonNodeDefinition<T> WithMap(Expression<Func<IJsonFieldMapper, T>> expression)
        {
            JsonMapperVisitor vis = new JsonMapperVisitor();
            vis.Visit(expression);
            foreach (var item in vis.MappingSetters)
                this.SetFieldDefinition(item);
            return this;
        }
        private void SetFieldDefinition(JsonFieldDefinition jsonFieldDefinition)
        {
            var existingFieldDefinition = _jsonFieldDefinitions.FirstOrDefault(i => i.TargetPropertyInfo.Name == jsonFieldDefinition.TargetPropertyInfo.Name);
            if (existingFieldDefinition == null)
                _jsonFieldDefinitions.Add(jsonFieldDefinition);
            else
                if (jsonFieldDefinition.NodePath != null) existingFieldDefinition.NodePath = jsonFieldDefinition.NodePath;
        }
    }
}
