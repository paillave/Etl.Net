using System;

namespace Paillave.Etl.Json.Core.Mapping.Visitors
{
    public class DummyFieldMapper : IJsonFieldMapper
    {
        public JsonFieldDefinition MappingSetter { get; } = new JsonFieldDefinition();

        public string ToSourceName()
        {
            this.MappingSetter.ForSourceName = true;
            return default;
        }

        public Guid ToRowGuid()
        {
            this.MappingSetter.ForRowGuid = true;
            return default;
        }

        public T ToPathQuery<T>(string xPathQuery)
        {
            this.MappingSetter.NodePath = xPathQuery;
            return default;
        }

        public T ToPathQuery<T>(string xPathQuery, int depthScope)
        {
            this.MappingSetter.NodePath = xPathQuery;
            this.MappingSetter.DepthScope = depthScope;
            return default;
        }
    }
}