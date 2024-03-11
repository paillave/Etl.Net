using Paillave.Etl.Json.Core.Mapping;

namespace Paillave.Etl.Json.Core
{
    public interface IJsonNodeDefinition
    {
        string Name { get; }
        string NodePath { get; }
        Type Type { get; }
        IList<JsonFieldDefinition> GetJsonFieldDefinitions();
    }
}
