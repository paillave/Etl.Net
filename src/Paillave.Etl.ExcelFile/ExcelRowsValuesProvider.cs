using OfficeOpenXml;
using Paillave.Etl.ExcelFile.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.IO;
using System.Linq;
using Paillave.Etl.Core;
using System.Collections.Generic;
using System.Threading;

namespace Paillave.Etl.ExcelFile
{
    public class ExcelRowsValuesProviderArgs<TIn, TParsed, TOut>
    {
        public Func<TIn, ExcelSheetSelection> GetSheetSelection { get; set; }
        public ExcelFileDefinition<TParsed> Mapping { get; set; }
        public Func<TParsed, TIn, TOut> GetOutput { get; set; }
    }

    public class ExcelRowsValuesProvider<TIn, TParsed, TOut>(ExcelRowsValuesProviderArgs<TIn, TParsed, TOut> args) : ValuesProviderBase<TIn, TOut>
    {
        private readonly ExcelRowsValuesProviderArgs<TIn, TParsed, TOut> _args = args;

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            var selection = _args.GetSheetSelection(input);
            // TODO: better exception handling here
            var reader = _args.Mapping.GetExcelReader(selection.ExcelWorksheet);
            int i = 0;
            bool foundRow = false;
            do
            {
                if (cancellationToken.IsCancellationRequested) break;
                IDictionary<string, object> row = new Dictionary<string, object>();
                foundRow = ReadRow(selection.ExcelWorksheet, reader, i++, row);
                if (foundRow) push(_args.GetOutput(ObjectBuilder<TParsed>.CreateInstance(row), input));
            } while (foundRow);
        }

        private bool ReadRow(ExcelWorksheet excelWorksheet, ExcelFileReader reader, int lineIndex, IDictionary<string, object> row)
        {
            //TODO: better exception handling here
            if (reader.DataRange == null)
            {
                throw new ArgumentNullException(nameof(reader.DataRange));
            }
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
