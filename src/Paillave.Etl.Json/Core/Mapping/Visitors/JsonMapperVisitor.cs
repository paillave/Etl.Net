using System.Linq.Expressions;

namespace Paillave.Etl.Json.Core.Mapping.Visitors
{
    public class JsonMapperVisitor : ExpressionVisitor
    {
        public List<JsonFieldDefinition> MappingSetters { get; private set; }
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            NewInstanceVisitor vis = new NewInstanceVisitor();
            vis.Visit(node.Body);

            this.MappingSetters = vis.MappingSetters;
            return null;
        }
    }
}
