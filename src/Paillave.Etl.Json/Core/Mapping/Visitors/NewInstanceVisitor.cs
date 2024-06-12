using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Json.Core.Mapping.Visitors
{
    public class NewInstanceVisitor : ExpressionVisitor
    {
        public List<JsonFieldDefinition> MappingSetters { get; private set; } = new List<JsonFieldDefinition>();

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Members != null)
                this.MappingSetters.AddRange(node.Members.Zip(node.Arguments, (m, a) => GetMappingSetterDefinition(a, m as PropertyInfo)).ToList());
            return base.VisitNew(node);
        }
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            var vis = new UnaryMapping();
            vis.Visit(node.Expression);
            if (vis.MappingSetter != null)
            {
                vis.MappingSetter.TargetPropertyInfo = node.Member as PropertyInfo;
                MappingSetters.Add(vis.MappingSetter);
            }
            else
            {
                var vis2 = new JsonMappingSetterVisitor();
                vis2.Visit(node.Expression);
                vis2.MappingSetter.TargetPropertyInfo = node.Member as PropertyInfo;
                MappingSetters.Add(vis2.MappingSetter);
            }
            return base.VisitMemberAssignment(node);
        }
        private JsonFieldDefinition GetMappingSetterDefinition(Expression argument, PropertyInfo propertyInfo)
        {
            JsonMappingSetterVisitor vis = new JsonMappingSetterVisitor();
            vis.Visit(argument);
            JsonFieldDefinition mappingSetter = vis.MappingSetter;
            mappingSetter.TargetPropertyInfo = propertyInfo;
            return mappingSetter;
        }
        private class UnaryMapping : ExpressionVisitor
        {
            public JsonFieldDefinition MappingSetter = null;
            protected override Expression VisitUnary(UnaryExpression node)
            {
                var vis = new JsonMappingSetterVisitor();
                vis.Visit(node.Operand);
                this.MappingSetter = vis.MappingSetter;
                return null;
            }
        }
    }
}
