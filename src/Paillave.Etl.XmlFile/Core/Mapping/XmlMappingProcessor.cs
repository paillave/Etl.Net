using Paillave.Etl.XmlFile.Core.Mapping.Visitors;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Paillave.Etl.XmlFile.Core.Mapping
{
    public class XmlMappingProcessor<T>
    {
        public readonly List<XmlFieldDefinition> _mappingSetters;

        public XmlMappingProcessor(Expression<Func<IXmlFieldMapper, T>> expression)
        {
            XmlMapperVisitor vis = new XmlMapperVisitor();
            vis.Visit(expression);
            this._mappingSetters = vis.MappingSetters;
        }
    }
}
