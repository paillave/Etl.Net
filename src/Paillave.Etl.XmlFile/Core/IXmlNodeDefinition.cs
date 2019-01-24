using Paillave.Etl.XmlFile.Core.Mapping;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.XmlFile.Core
{
    public interface IXmlNodeDefinition
    {
        string Name { get; }
        string NodeXPath { get; }
        Type Type { get; }
        IList<XmlFieldDefinition> GetXmlFieldDefinitions();
    }
}
