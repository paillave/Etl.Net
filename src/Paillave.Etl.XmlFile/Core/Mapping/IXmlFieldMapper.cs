using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.XmlFile.Core.Mapping
{
    public interface IXmlFieldMapper
    {
        T ToXPathQuery<T>(string xPathQuery);
    }
}
