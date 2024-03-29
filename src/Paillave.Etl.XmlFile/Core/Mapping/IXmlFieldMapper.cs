﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.XmlFile.Core.Mapping
{
    public interface IXmlFieldMapper
    {
        T ToXPathQuery<T>(string xPathQuery);
        T ToXPathQuery<T>(string xPathQuery, int depthScope);
        string ToSourceName();
        Guid ToRowGuid();
    }
}
