using OfficeOpenXml;
using Paillave.Etl.Core;
using Paillave.Etl.ExcelFile.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.IO;
using System.Linq;

namespace Paillave.Etl.ExcelFile.ValuesProviders
{
    public class ExcelRowsValuesProviderArgs<TParsed> where TParsed : new()
    {
        public bool NoParallelisation { get; set; } = false;
        public ExcelFileDefinition<TParsed> Mapping { get; set; }
    }
    public class ExcelRowsValuesProvider<TParsed> : ValuesProviderBase<ExcelSheetSelection, TParsed> where TParsed : new() //<TOut> : ValuesProviderBase<ExcelSheetSelection, TOut>
    {
        private ExcelRowsValuesProviderArgs<TParsed> _args;
        public ExcelRowsValuesProvider(ExcelRowsValuesProviderArgs<TParsed> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        protected override void PushValues(ExcelSheetSelection input, Action<TParsed> pushValue)
        {
            using (base.OpenProcess())
            {
                var reader = _args.Mapping.GetExcelReader(input.ExcelWorksheet);
                int i = 0;
                bool foundRow = false;
                do
                {
                    TParsed row = new TParsed();
                    foundRow = ReadRow(input.ExcelWorksheet, reader, i++, row);
                    if (foundRow) pushValue(row);
                } while (foundRow);
            }
        }
        private bool ReadRow(ExcelWorksheet excelWorksheet, ExcelFileReader reader, int lineIndex, TParsed row)
        {
            bool isEmptyRow = true;
            switch (reader.DatasetOrientation)
            {
                case DataOrientation.Horizontal:
                    for (int i = 0; i < reader.DataRange.Rows; i++)
                    {
                        if (reader.PropertySetters[i].SetValue(excelWorksheet, row, i + reader.DataRange.Start.Row, lineIndex + reader.DataRange.Start.Column))
                            isEmptyRow = false;
                    }
                    break;
                case DataOrientation.Vertical:
                    for (int i = 0; i < reader.DataRange.Columns; i++)
                        if (reader.PropertySetters[i].SetValue(excelWorksheet, row, lineIndex + reader.DataRange.Start.Row, i + reader.DataRange.Start.Column))
                            isEmptyRow = false;
                    break;
                default:
                    break;
            }
            return !isEmptyRow;
        }
    }
}