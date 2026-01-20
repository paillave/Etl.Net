
using Paillave.Etl.Core.Mapping.Visitors;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.Mapping;

public class MappingProcessor<T>
{
    public readonly List<MappingSetterDefinition> MappingSetters;

    public MappingProcessor(Expression<Func<IFieldMapper, T>> expression)
    {
        MapperVisitor vis = new();
        vis.Visit(expression);
        this.MappingSetters = vis.MappingSetters;
    }
}
//public static class MappingProcessor
//{
//    public static MappingProcessor<T> Create<T>(Expression<Func<IFieldMapper, T>> expression) => new MappingProcessor<T>(expression);
//}
