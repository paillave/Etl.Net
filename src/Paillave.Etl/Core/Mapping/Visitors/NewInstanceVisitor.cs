using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Mapping.Visitors
{
    public class NewInstanceVisitor : ExpressionVisitor
    {
        public List<MappingSetterDefinition> MappingSetters { get; private set; }

        protected override Expression VisitNew(NewExpression node)
        {
            this.MappingSetters = node.Members.Zip(node.Arguments, (m, a) => GetMappingSetterDefinition(a, m as PropertyInfo)).ToList();
            return null;
        }
        private MappingSetterDefinition GetMappingSetterDefinition(Expression argument, PropertyInfo propertyInfo)
        {
            MappingSetterVisitor vis = new MappingSetterVisitor();
            vis.Visit(argument);
            MappingSetterDefinition mappingSetter = vis.MappingSetter;
            mappingSetter.TargetPropertyInfo = propertyInfo;
            return mappingSetter;
        }
    }
}
