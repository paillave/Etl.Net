using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Paillave.Etl.Core.Helpers
{
    public class PropertyMap
    {
        public PropertyInfo PropertyInfo { get; set; }
        public TypeConverter TypeConverter { get; set; }
        public CultureInfo CultureInfo { get; set; }
    }
}
