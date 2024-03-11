using Paillave.Etl.Json.Core.Mapping.Visitors;
using System.Linq.Expressions;

namespace Paillave.Etl.Json.Core.Mapping
{
    public class JsonMappingProcessor<T>
    {
        public readonly List<JsonFieldDefinition> _mappingSetters;

        public JsonMappingProcessor(Expression<Func<IJsonFieldMapper, T>> expression)
        {
            JsonMapperVisitor vis = new JsonMapperVisitor();
            vis.Visit(expression);
            this._mappingSetters = vis.MappingSetters;
        }
    }
}
