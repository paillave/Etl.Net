using OfficeOpenXml;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.IO;
using System.Linq;

namespace Paillave.Etl.ExcelFile.ValuesProviders
{
    public class ExcelSheetsValuesProviderArgs<TIn>
    {
        public Func<TIn, Stream> DataStreamSelector { get; set; }
        public bool NoParallelisation { get; set; } = false;
    }
    public class ExcelSheetSelection
    {
        internal ExcelWorksheet ExcelWorksheet { get; }
        internal ExcelSheetSelection(ExcelWorksheet excelWorksheet)
        {
            ExcelWorksheet = excelWorksheet;
        }
        public string Name => this.ExcelWorksheet.Name;
    }
    public class ExcelSheetsValuesProvider<TIn> : ValuesProviderBase<TIn, ExcelSheetSelection>
    {
        private ExcelSheetsValuesProviderArgs<TIn> _args;
        public ExcelSheetsValuesProvider(ExcelSheetsValuesProviderArgs<TIn> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        protected override void PushValues(TIn input, Action<ExcelSheetSelection> pushValue)
        {
            using (base.OpenProcess())
            {
                var pck = new ExcelPackage(_args.DataStreamSelector(input));
                foreach (var worksheet in pck.Workbook.Worksheets)
                    pushValue(new ExcelSheetSelection(worksheet));
            }
        }
    }
}