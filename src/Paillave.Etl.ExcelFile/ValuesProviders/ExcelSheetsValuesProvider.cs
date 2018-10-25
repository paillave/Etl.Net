using OfficeOpenXml;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.IO;
using System.Linq;

namespace Paillave.Etl.ExcelFile.ValuesProviders
{
    public class ExcelSheetSelection
    {
        internal ExcelWorksheet ExcelWorksheet { get; }
        internal ExcelSheetSelection(ExcelWorksheet excelWorksheet)
        {
            ExcelWorksheet = excelWorksheet;
        }
        public string Name => this.ExcelWorksheet.Name;
    }
    public class ExcelSheetsValuesProvider
    {
        public void PushValues(Stream input, Action<ExcelSheetSelection> pushValue)
        {
            var pck = new ExcelPackage(input);
            foreach (var worksheet in pck.Workbook.Worksheets)
                pushValue(new ExcelSheetSelection(worksheet));
        }
        public void PushValues(string excelFilePath, Action<ExcelSheetSelection> pushValue)
        {
            using (var stream = File.OpenRead(excelFilePath))
                PushValues(stream, pushValue);
        }
        public void PushValues<TIn>(TIn input, Func<TIn, string> getExcelFilePath, Action<ExcelSheetSelection> pushValue)
        {
            PushValues(getExcelFilePath(input), pushValue);
        }
    }
}
