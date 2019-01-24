using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.XmlFile.Core.Mapping.Visitors
{
    public class NewInstanceVisitor : ExpressionVisitor
    {
        public List<XmlFieldDefinition> MappingSetters { get; private set; } = new List<XmlFieldDefinition>();

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
                var vis2 = new XmlMappingSetterVisitor();
                vis2.Visit(node.Expression);
                vis2.MappingSetter.TargetPropertyInfo = node.Member as PropertyInfo;
                MappingSetters.Add(vis2.MappingSetter);
            }
            return base.VisitMemberAssignment(node);
        }
        private XmlFieldDefinition GetMappingSetterDefinition(Expression argument, PropertyInfo propertyInfo)
        {
            XmlMappingSetterVisitor vis = new XmlMappingSetterVisitor();
            vis.Visit(argument);
            XmlFieldDefinition mappingSetter = vis.MappingSetter;
            mappingSetter.TargetPropertyInfo = propertyInfo;
            return mappingSetter;
        }
        private class UnaryMapping : ExpressionVisitor
        {
            public XmlFieldDefinition MappingSetter = null;
            protected override Expression VisitUnary(UnaryExpression node)
            {
                var vis = new XmlMappingSetterVisitor();
                vis.Visit(node.Operand);
                this.MappingSetter = vis.MappingSetter;
                return null;
            }
        }
    }
}
