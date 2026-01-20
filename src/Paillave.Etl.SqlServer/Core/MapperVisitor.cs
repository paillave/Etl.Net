using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Paillave.Etl.SqlServer.Core;

public class MapperVisitor : ExpressionVisitor
{
    public List<SqlResultFieldDefinition> MappingSetters { get; private set; }
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        NewInstanceVisitor vis = new();
        vis.Visit(node.Body);

        // MemberBindingVisitor vis2 = new MemberBindingVisitor();
        // vis2.Visit(node.Body);

        this.MappingSetters = vis.MappingSetters;
        return null;
    }
}
