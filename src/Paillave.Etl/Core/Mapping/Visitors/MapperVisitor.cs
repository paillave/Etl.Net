using System.Collections.Generic;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.Mapping.Visitors;

public class MapperVisitor : ExpressionVisitor
{
    public List<MappingSetterDefinition> MappingSetters { get; private set; }
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        NewInstanceVisitor vis = new();
        vis.Visit(node.Body);
        this.MappingSetters = vis.MappingSetters;
        return null;
    }
}
