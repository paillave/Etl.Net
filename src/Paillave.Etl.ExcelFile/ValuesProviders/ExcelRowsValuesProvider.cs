using OfficeOpenXml;
using Paillave.Etl.ExcelFile.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.IO;
using System.Linq;
using Paillave.Etl.Core;
using System.Collections.Generic;

namespace Paillave.Etl.ExcelFile.ValuesProviders
{
    public class ExcelRowsValuesProvider<TParsed>
    {
        private ExcelFileDefinition<TParsed> _mapping;
        public ExcelRowsValuesProvider(ExcelFileDefinition<TParsed> mapping)
        {
            _mapping = mapping;
        }
        public void PushValues(ExcelSheetSelection input, Action<TParsed> pushValue)
        {
            //TODO: better exception handling here
            var reader = _mapping.GetExcelReader(input.ExcelWorksheet);
            int i = 0;
            bool foundRow = false;
            do
            {
                IDictionary<string, object> row = new Dictionary<string, object>();
                foundRow = ReadRow(input.ExcelWorksheet, reader, i++, row);
                if (foundRow) pushValue(ObjectBuilder<TParsed>.CreateInstance(row));
            } while (foundRow);
        }
        private bool ReadRow(ExcelWorksheet excelWorksheet, ExcelFileReader reader, int lineIndex, IDictionary<string, object> row)
        {
            //TODO: better exception handling here
            bool isEmptyRow = true;
            switch (reader.DatasetOrientation)
            {
                case DataOrientation.Horizontal:
                    for (int i = 0; i < reader.DataRange.Rows; i++)
                    {
                        if (reader.PropertySetters.TryGetValue(i, out var propertySetter))
                        {
                            if (propertySetter.SetValue(excelWorksheet, i + reader.DataRange.Start.Row, lineIndex + reader.DataRange.Start.Column))
                            {
                                isEmptyRow = false;
                                row[propertySetter.PropertyInfo.Name] = propertySetter.ParsedValue;
                            }
                        }
                    }
                    break;
                case DataOrientation.Vertical:
                    for (int i = 0; i < reader.DataRange.Columns; i++)
                    {
                        if (reader.PropertySetters.TryGetValue(i, out var propertySetter))
                        {
                            if (propertySetter.SetValue(excelWorksheet, lineIndex + reader.DataRange.Start.Row, i + reader.DataRange.Start.Column))
                            {
                                isEmptyRow = false;
                                row[propertySetter.PropertyInfo.Name] = propertySetter.ParsedValue;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            return !isEmptyRow;
        }
    }
}
