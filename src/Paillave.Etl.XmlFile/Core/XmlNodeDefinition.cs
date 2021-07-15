using Paillave.Etl.XmlFile.Core.Mapping;
using Paillave.Etl.XmlFile.Core.Mapping.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.XmlFile.Core
{
    public static class XmlNodeDefinition
    {
        public static XmlNodeDefinition<T> Create<T>(string name, string nodeXPath, Expression<Func<IXmlFieldMapper, T>> expression)
            => new XmlNodeDefinition<T>(name, nodeXPath).WithMap(expression);
    }
    public class XmlNodeDefinition<T> : IXmlNodeDefinition
    {
        public string Name { get; set; }
        public IList<XmlFieldDefinition> _xmlFieldDefinitions = new List<XmlFieldDefinition>();

        public IList<XmlFieldDefinition> GetXmlFieldDefinitions() => _xmlFieldDefinitions.ToList();
        public string NodePath { get; private set; }

        public Type Type { get; } = typeof(T);

        public XmlNodeDefinition(string name, string nodeXPath)
        {
            this.Name = name;
            this.NodePath = nodeXPath;
        }
        public XmlNodeDefinition<T> WithMap(Expression<Func<IXmlFieldMapper, T>> expression)
        {
            XmlMapperVisitor vis = new XmlMapperVisitor();
            vis.Visit(expression);
            foreach (var item in vis.MappingSetters)
                this.SetFieldDefinition(item);
            return this;
        }
        private void SetFieldDefinition(XmlFieldDefinition xmlFieldDefinition)
        {
            var existingFieldDefinition = _xmlFieldDefinitions.FirstOrDefault(i => i.TargetPropertyInfo.Name == xmlFieldDefinition.TargetPropertyInfo.Name);
            if (existingFieldDefinition == null)
                _xmlFieldDefinitions.Add(xmlFieldDefinition);
            else
                if (xmlFieldDefinition.NodePath != null) existingFieldDefinition.NodePath = xmlFieldDefinition.NodePath;
        }
        // public XmlNodeDefinition<T> MapXPathToProperty<TField>(string valueXPathQuery, Expression<Func<T, TField>> memberLambda)
        // {
        //     SetFieldDefinition(new XmlFieldDefinition
        //     {
        //         NodePath = valueXPathQuery,
        //         TargetPropertyInfo = memberLambda.GetPropertyInfo()
        //     });
        //     return this;
        // }
    }
}
