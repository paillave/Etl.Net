using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.ExcelFile.Core
{
    public class ExcelFileReader
    {
        public ExcelFileReader(IDictionary<int, ExcelFilePropertySetter> propertySetters, ExcelAddressBase columnHeaderRange, ExcelAddressBase dataRange, DataOrientation datasetOrientation)
        {
            PropertySetters = propertySetters;
            ColumnHeaderRange = columnHeaderRange;
            DataRange = dataRange;
            DatasetOrientation = datasetOrientation;
        }

        public IDictionary<int, ExcelFilePropertySetter> PropertySetters { get; }
        public ExcelAddressBase ColumnHeaderRange { get; }
        public ExcelAddressBase DataRange { get; }
        public DataOrientation DatasetOrientation { get; }
        public Dictionary<string, string> GetTextMapping()
            => PropertySetters.Values.ToDictionary(i => i.PropertyInfo.Name, i => i.Column);
    }
}
