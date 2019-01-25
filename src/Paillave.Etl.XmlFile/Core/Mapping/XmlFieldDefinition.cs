using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Paillave.Etl.XmlFile.Core.Mapping
{
    public class XmlFieldDefinition
    {
        public int DepthScope { get; internal set; } = 0;
        public string XPathQuery { get; internal set; } = null;
        public PropertyInfo TargetPropertyInfo { get; internal set; }
    }
}
