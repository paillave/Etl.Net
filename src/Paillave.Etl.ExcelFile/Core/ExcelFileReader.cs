using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.ExcelFile.Core;

public class ExcelFileReader(IDictionary<int, ExcelFilePropertySetter> propertySetters, ExcelAddressBase columnHeaderRange, ExcelAddressBase dataRange, DataOrientation datasetOrientation)
{
    public IDictionary<int, ExcelFilePropertySetter> PropertySetters { get; } = propertySetters;
    public ExcelAddressBase ColumnHeaderRange { get; } = columnHeaderRange;
    public ExcelAddressBase DataRange { get; } = dataRange;
    public DataOrientation DatasetOrientation { get; } = datasetOrientation;
    public Dictionary<string, string> GetTextMapping()
        => PropertySetters.Values.ToDictionary(i => i.PropertyInfo.Name, i => i.Column);
}
