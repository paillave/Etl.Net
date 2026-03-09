using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Paillave.Etl.XmlFile.Core.Mapping.Visitors;

public class XmlMapperVisitor : ExpressionVisitor
{
    public List<XmlFieldDefinition> MappingSetters { get; private set; }
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        NewInstanceVisitor vis = new();
        vis.Visit(node.Body);

        this.MappingSetters = vis.MappingSetters;
        return null;
    }
}
