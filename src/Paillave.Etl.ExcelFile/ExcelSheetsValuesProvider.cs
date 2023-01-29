using OfficeOpenXml;
using Paillave.Etl.Core;
using System;
using System.Threading;

namespace Paillave.Etl.ExcelFile
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
    public class ExcelSheetsValuesProviderArgs<TOut>
    {
        public Func<ExcelSheetSelection, IFileValue, TOut> GetOutput { get; set; }
        public bool UseStreamCopy { get; set; } = true;
    }
    public class ExcelSheetsValuesProvider<TOut> : ValuesProviderBase<IFileValue, TOut>
    {
        private ExcelSheetsValuesProviderArgs<TOut> _args;
        public ExcelSheetsValuesProvider(ExcelSheetsValuesProviderArgs<TOut> args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        public override void PushValues(IFileValue input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            using var stream = input.Get(_args.UseStreamCopy);
            context.AddUnderlyingDisposables(stream);
            var pck = new ExcelPackage(stream);
            foreach (var worksheet in pck.Workbook.Worksheets)
            {
                if (cancellationToken.IsCancellationRequested) break;
                push(_args.GetOutput(new ExcelSheetSelection(worksheet), input));
            }
        }
    }
}
