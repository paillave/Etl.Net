using Paillave.Etl.Core;
using Paillave.Etl.ExcelFile.Core;
using System;
using System.IO;

namespace Paillave.Etl.ExcelFile
{
    public static class ExcelFileEx
    {
        #region CrossApplyExcelSheets
        public static IStream<ExcelSheetSelection> CrossApplyExcelSheets(this IStream<IFileValue> stream, string name, bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelSheetsValuesProvider<ExcelSheetSelection>(new ExcelSheetsValuesProviderArgs<ExcelSheetSelection>
            {
                GetOutput = (i, j) => i
            }), noParallelisation);
        public static IStream<TOut> CrossApplyExcelSheets<TOut>(this IStream<IFileValue> stream, string name, Func<ExcelSheetSelection, TOut> selector, bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelSheetsValuesProvider<TOut>(new ExcelSheetsValuesProviderArgs<TOut>
            {
                GetOutput = (i, j) => selector(i)
            }), noParallelisation);
        #endregion

        #region CrossApplyExcelRows
        public static IStream<TOut> CrossApplyExcelRows<TParsed, TOut>(this IStream<ExcelSheetSelection> stream, string name, ExcelFileDefinition<TParsed> mapping, Func<TParsed, ExcelSheetSelection, TOut> selector, bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<ExcelSheetSelection, TParsed, TOut>(new ExcelRowsValuesProviderArgs<ExcelSheetSelection, TParsed, TOut>
            {
                Mapping = mapping,
                GetSheetSelection = i => i,
                GetOutput = selector
            }), noParallelisation);
        public static IStream<TOut> CrossApplyExcelRows<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, ExcelFileDefinition<TParsed> mapping, Func<TIn, ExcelSheetSelection> sheetSelection, Func<TParsed, TIn, TOut> selector, bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<TIn, TParsed, TOut>(new ExcelRowsValuesProviderArgs<TIn, TParsed, TOut>
            {
                Mapping = mapping,
                GetSheetSelection = sheetSelection,
                GetOutput = selector
            }), noParallelisation);
        public static IStream<TParsed> CrossApplyExcelRows<TIn, TParsed>(this IStream<TIn> stream, string name, ExcelFileDefinition<TParsed> mapping, Func<TIn, ExcelSheetSelection> sheetSelection, bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<TIn, TParsed, TParsed>(new ExcelRowsValuesProviderArgs<TIn, TParsed, TParsed>
            {
                Mapping = mapping,
                GetSheetSelection = sheetSelection,
                GetOutput = (i, j) => i
            }), noParallelisation);
        public static IStream<TParsed> CrossApplyExcelRows<TParsed>(this IStream<ExcelSheetSelection> stream, string name, ExcelFileDefinition<TParsed> mapping, bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<ExcelSheetSelection, TParsed, TParsed>(new ExcelRowsValuesProviderArgs<ExcelSheetSelection, TParsed, TParsed>
            {
                Mapping = mapping,
                GetSheetSelection = i => i,
                GetOutput = (i, j) => i
            }), noParallelisation);
        #endregion

        #region ThroughExcelFile
        public static IStream<TIn> ToExcelFile<TIn>(this IStream<TIn> stream, string name, ISingleStream<Stream> resourceStream, ExcelFileDefinition<TIn> mapping = null)
        {
            return new ToExcelFileStreamNode<TIn, IStream<TIn>>(name, new ToExcelFileArgs<TIn, IStream<TIn>>
            {
                MainStream = stream,
                TargetStream = resourceStream,
                Mapping = mapping
            }).Output;
        }
        public static ISortedStream<TIn, TKey> ToExcelFile<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, ISingleStream<Stream> resourceStream, ExcelFileDefinition<TIn> mapping = null)
        {
            return new ToExcelFileStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ToExcelFileArgs<TIn, ISortedStream<TIn, TKey>>
            {
                MainStream = stream,
                TargetStream = resourceStream,
                Mapping = mapping
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> ToExcelFile<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, ISingleStream<Stream> resourceStream, ExcelFileDefinition<TIn> mapping = null)
        {
            return new ToExcelFileStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ToExcelFileArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                MainStream = stream,
                TargetStream = resourceStream,
                Mapping = mapping
            }).Output;
        }
        #endregion

        #region ToExcelFile
        public static IStream<IFileValue> ToExcelFile<TIn>(this IStream<TIn> stream, string name, string fileName, ExcelFileDefinition<TIn> mapping = null)
        {
            return new ToExcelFileStreamNode<TIn>(name, new ToExcelFileArgs<TIn>
            {
                MainStream = stream,
                Mapping = mapping,
                FileName = fileName
            }).Output;
        }
        #endregion
    }
}
