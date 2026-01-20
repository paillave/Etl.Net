using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Mapping.Visitors;

public class NewInstanceVisitor : ExpressionVisitor
{
    public List<MappingSetterDefinition> MappingSetters { get; private set; } = new List<MappingSetterDefinition>();

    protected override Expression VisitNew(NewExpression node)
    {
        if (node.Members != null)
            this.MappingSetters.AddRange(node.Members.Zip(node.Arguments, (m, a) => GetMappingSetterDefinition(a, m as PropertyInfo)).Where(i => i != null).Cast<MappingSetterDefinition>().ToList());
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
            var vis2 = new MappingSetterVisitor();
            vis2.Visit(node.Expression);
            if (vis2.MappingSetter != null)
            {
                vis2.MappingSetter.TargetPropertyInfo = node.Member as PropertyInfo;
                MappingSetters.Add(vis2.MappingSetter);
            }
        }
        return base.VisitMemberAssignment(node);
    }
    private MappingSetterDefinition? GetMappingSetterDefinition(Expression argument, PropertyInfo? propertyInfo)
    {
        MappingSetterVisitor vis = new();
        vis.Visit(argument);
        var mappingSetter = vis.MappingSetter;
        if (mappingSetter != null)
        {
            mappingSetter.TargetPropertyInfo = propertyInfo;
        }
        return mappingSetter;
    }
    private class UnaryMapping : ExpressionVisitor
    {
        public MappingSetterDefinition? MappingSetter = null;
        protected override Expression? VisitUnary(UnaryExpression node)
        {
            var vis = new MappingSetterVisitor();
            vis.Visit(node.Operand);
            this.MappingSetter = vis.MappingSetter;
            return null;
        }
    }
}
