using Paillave.Etl.Core.Streams;
using Paillave.Etl.ExcelFile.ValuesProviders;
using System;
using System.IO;
using SystemIO = System.IO;

namespace Paillave.Etl
{
    public static class StreamExXl
    {
        #region CrossApplyExcelSheets
        public static IStream<ExcelSheetSelection> CrossApplyExcelSheets<TIn>(this IStream<TIn> stream, string name, Func<TIn, string> getExcelFilePath)
        {
            return stream.CrossApply(name, new ExcelSheetsValuesProvider<TIn>(new ExcelSheetsValuesProviderArgs<TIn>
            {
                DataStreamSelector = i => File.OpenRead(getExcelFilePath(i)),
                NoParallelisation = false
            }), i => i, (i, j) => i);
        }
        public static IStream<ExcelSheetSelection> CrossApplyExcelSheets(this IStream<string> stream, string name)
        {
            return stream.CrossApply(name, new ExcelSheetsValuesProvider<string>(new ExcelSheetsValuesProviderArgs<string>
            {
                DataStreamSelector = i => File.OpenRead(i),
                NoParallelisation = false
            }), i => i, (i, j) => i);
        }
        public static IStream<ExcelSheetSelection> CrossApplyExcelSheets(this IStream<Stream> stream, string name)
        {
            return stream.CrossApply(name, new ExcelSheetsValuesProvider<Stream>(new ExcelSheetsValuesProviderArgs<Stream>
            {
                DataStreamSelector = i => i,
                NoParallelisation = false
            }), i => i, (i, j) => i);
        }
        public static IStream<TOut> CrossApplyExcelSheets<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, string> getExcelFilePath, Func<ExcelSheetSelection, TIn, TOut> selector)
        {
            return stream.CrossApply(name, new ExcelSheetsValuesProvider<TIn>(new ExcelSheetsValuesProviderArgs<TIn>
            {
                DataStreamSelector = i => File.OpenRead(getExcelFilePath(i)),
                NoParallelisation = false
            }), i => i, selector);
        }
        public static IStream<TOut> CrossApplyExcelSheets<TOut>(this IStream<string> stream, string name, Func<ExcelSheetSelection, string, TOut> selector)
        {
            return stream.CrossApply(name, new ExcelSheetsValuesProvider<string>(new ExcelSheetsValuesProviderArgs<string>
            {
                DataStreamSelector = i => File.OpenRead(i),
                NoParallelisation = false
            }), i => i, selector);
        }
        #endregion
    }
}
