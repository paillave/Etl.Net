using OfficeOpenXml;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.IO;
using System.Linq;

namespace Paillave.Etl.ExcelFile.ValuesProviders
{
    public class ExcelRowsValuesProviderArgs<TOut>
    {
        public bool NoParallelisation { get; set; } = false;

    }
    public class ExcelRowsValuesProvider<TOut> : ValuesProviderBase<ExcelSheetSelection, TOut>
    {
        private ExcelRowsValuesProviderArgs<TOut> _args;
        public ExcelRowsValuesProvider(ExcelRowsValuesProviderArgs<TOut> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        protected override void PushValues(ExcelSheetSelection input, Action<TOut> pushValue)
        {
            using (base.OpenProcess())
            {
                //var pck = new ExcelPackage(_args.DataStreamSelector(input));
                //foreach (var worksheet in pck.Workbook.Worksheets)
                //    pushValue(new ExcelSheetSelection(worksheet));
            }
        }
    }
}