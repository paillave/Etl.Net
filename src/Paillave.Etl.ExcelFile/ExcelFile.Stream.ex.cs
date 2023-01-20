using Paillave.Etl.Core;
using Paillave.Etl.Core.Mapping;
using Paillave.Etl.ExcelFile.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;

namespace Paillave.Etl.ExcelFile
{
    public class ExcelFileArgBuilder
    {
        public ExcelFileDefinition<T> UseMap<T>(Expression<Func<IFieldMapper, T>> expression) => ExcelFileDefinition.Create(expression);
        public ExcelFileDefinition<T> UseType<T>() => new ExcelFileDefinition<T>();
        public ExcelFileDefinition<T> UseType<T>(T prototype) => new ExcelFileDefinition<T>();
    }

    public static class ExcelFileEx
    {
        #region CrossApplyExcelSheets
        public static IStream<ExcelSheetSelection> CrossApplyExcelSheets(
            this IStream<IFileValue> stream,
            string name,
            bool noParallelisation = false,
            bool useStreamCopy = false)
            => stream.CrossApply(name, new ExcelSheetsValuesProvider<ExcelSheetSelection>(new ExcelSheetsValuesProviderArgs<ExcelSheetSelection>
            {
                GetOutput = (i, j) => i,
                UseStreamCopy = useStreamCopy
            }), noParallelisation);
        public static IStream<TOut> CrossApplyExcelSheets<TOut>(
            this IStream<IFileValue> stream,
            string name,
            Func<ExcelSheetSelection, TOut> selector,
            bool noParallelisation = false,
            bool useStreamCopy = false)
            => stream.CrossApply(name, new ExcelSheetsValuesProvider<TOut>(new ExcelSheetsValuesProviderArgs<TOut>
            {
                GetOutput = (i, j) => selector(i),
                UseStreamCopy = useStreamCopy
            }), noParallelisation);
        public static IStream<TOut> CrossApplyExcelDatasets<TOut>(
            this IStream<IFileValue> stream,
            string name,
            Func<DataTable, IFileValue, IEnumerable<TOut>> selector,
            bool noParallelisation = false,
            bool useStreamCopy = false)
            => stream.CrossApply(name, new ExcelDatasetsValuesProvider<TOut>(new ExcelDatasetsValuesProviderArgs<TOut>
            {
                GetOutput = selector,
                UseStreamCopy = useStreamCopy
            }), noParallelisation);
        #endregion


        #region CrossApplyExcelDatasets
        public static IStream<DataTable> CrossApplyExcelDataTables(
            this IStream<IFileValue> stream,
            string name,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelDataTablesValuesProvider(), noParallelisation);
        public static IStream<TOut> CrossApplyExcelDataTables<TOut>(
            this IStream<IFileValue> stream,
            string name,
            Func<DataTable, IEnumerable<TOut>> selector,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelDatasetsValuesProvider<TOut>(new ExcelDatasetsValuesProviderArgs<TOut>
            {
                GetOutput = (i, j) => selector(i)
            }), noParallelisation);
        #endregion

        #region CrossApplyExcelRows
        public static IStream<TOut> CrossApplyExcelRows<TParsed, TOut>(
            this IStream<ExcelSheetSelection> stream,
            string name,
            Func<ExcelFileArgBuilder, ExcelFileDefinition<TParsed>> mapBuilder,
            Func<TParsed, ExcelSheetSelection, TOut> selector,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<ExcelSheetSelection, TParsed, TOut>(new ExcelRowsValuesProviderArgs<ExcelSheetSelection, TParsed, TOut>
            {
                Mapping = mapBuilder(new()),
                GetSheetSelection = i => i,
                GetOutput = selector
            }), noParallelisation);
        public static IStream<TOut> CrossApplyExcelRows<TParsed, TOut>(
            this IStream<ExcelSheetSelection> stream,
            string name,
            ExcelFileDefinition<TParsed> mapping,
            Func<TParsed, ExcelSheetSelection, TOut> selector,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<ExcelSheetSelection, TParsed, TOut>(new ExcelRowsValuesProviderArgs<ExcelSheetSelection, TParsed, TOut>
            {
                Mapping = mapping,
                GetSheetSelection = i => i,
                GetOutput = selector
            }), noParallelisation);
        public static IStream<TOut> CrossApplyExcelRows<TIn, TParsed, TOut>(
            this IStream<TIn> stream,
            string name,
            Func<ExcelFileArgBuilder, ExcelFileDefinition<TParsed>> mapBuilder,
            Func<TIn, ExcelSheetSelection> sheetSelection,
            Func<TParsed, TIn, TOut> selector,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<TIn, TParsed, TOut>(new ExcelRowsValuesProviderArgs<TIn, TParsed, TOut>
            {
                Mapping = mapBuilder(new()),
                GetSheetSelection = sheetSelection,
                GetOutput = selector
            }), noParallelisation);
        public static IStream<TOut> CrossApplyExcelRows<TIn, TParsed, TOut>(
            this IStream<TIn> stream,
            string name,
            ExcelFileDefinition<TParsed> mapping,
            Func<TIn, ExcelSheetSelection> sheetSelection,
            Func<TParsed, TIn, TOut> selector,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<TIn, TParsed, TOut>(new ExcelRowsValuesProviderArgs<TIn, TParsed, TOut>
            {
                Mapping = mapping,
                GetSheetSelection = sheetSelection,
                GetOutput = selector
            }), noParallelisation);
        public static IStream<TParsed> CrossApplyExcelRows<TIn, TParsed>(
            this IStream<TIn> stream,
            string name,
            Func<ExcelFileArgBuilder, ExcelFileDefinition<TParsed>> mapBuilder,
            Func<TIn, ExcelSheetSelection> sheetSelection,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<TIn, TParsed, TParsed>(new ExcelRowsValuesProviderArgs<TIn, TParsed, TParsed>
            {
                Mapping = mapBuilder(new()),
                GetSheetSelection = sheetSelection,
                GetOutput = (i, j) => i
            }), noParallelisation);
        public static IStream<TParsed> CrossApplyExcelRows<TIn, TParsed>(
            this IStream<TIn> stream,
            string name,
            ExcelFileDefinition<TParsed> mapping,
            Func<TIn, ExcelSheetSelection> sheetSelection,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<TIn, TParsed, TParsed>(new ExcelRowsValuesProviderArgs<TIn, TParsed, TParsed>
            {
                Mapping = mapping,
                GetSheetSelection = sheetSelection,
                GetOutput = (i, j) => i
            }), noParallelisation);
        public static IStream<TParsed> CrossApplyExcelRows<TParsed>(
            this IStream<ExcelSheetSelection> stream,
            string name,
            Func<ExcelFileArgBuilder, ExcelFileDefinition<TParsed>> mapBuilder,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<ExcelSheetSelection, TParsed, TParsed>(new ExcelRowsValuesProviderArgs<ExcelSheetSelection, TParsed, TParsed>
            {
                Mapping = mapBuilder(new()),
                GetSheetSelection = i => i,
                GetOutput = (i, j) => i
            }), noParallelisation);
        public static IStream<TParsed> CrossApplyExcelRows<TParsed>(
            this IStream<ExcelSheetSelection> stream,
            string name,
            ExcelFileDefinition<TParsed> mapping,
            bool noParallelisation = false)
            => stream.CrossApply(name, new ExcelRowsValuesProvider<ExcelSheetSelection, TParsed, TParsed>(new ExcelRowsValuesProviderArgs<ExcelSheetSelection, TParsed, TParsed>
            {
                Mapping = mapping,
                GetSheetSelection = i => i,
                GetOutput = (i, j) => i
            }), noParallelisation);
        #endregion

        #region ThroughExcelFile
        [Obsolete]
        public static IStream<TIn> ToExcelFile<TIn>(
            this IStream<TIn> stream,
            string name,
            ISingleStream<Stream> resourceStream,
            ExcelFileDefinition<TIn> mapping = null)
            => new ToExcelFileStreamNode<TIn, IStream<TIn>>(name, new ToExcelFileArgs<TIn, IStream<TIn>>
            {
                MainStream = stream,
                TargetStream = resourceStream,
                Mapping = mapping
            }).Output;
        [Obsolete]
        public static ISortedStream<TIn, TKey> ToExcelFile<TIn, TKey>(
            this ISortedStream<TIn, TKey> stream,
            string name,
            ISingleStream<Stream> resourceStream,
            ExcelFileDefinition<TIn> mapping = null)
            => new ToExcelFileStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new ToExcelFileArgs<TIn, ISortedStream<TIn, TKey>>
            {
                MainStream = stream,
                TargetStream = resourceStream,
                Mapping = mapping
            }).Output;
        [Obsolete]
        public static IKeyedStream<TIn, TKey> ToExcelFile<TIn, TKey>(
            this IKeyedStream<TIn, TKey> stream,
            string name,
            ISingleStream<Stream> resourceStream,
            ExcelFileDefinition<TIn> mapping = null)
            => new ToExcelFileStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new ToExcelFileArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                MainStream = stream,
                TargetStream = resourceStream,
                Mapping = mapping
            }).Output;

        #endregion

        #region ToExcelFile
        public static IStream<IFileValue> ToExcelFile<TIn>(
            this IStream<TIn> stream,
            string name,
            string fileName,
            ExcelFileDefinition<TIn> mapping = null)
            => new ToExcelFileStreamNode<TIn>(name, new ToExcelFileArgs<TIn>
            {
                MainStream = stream,
                Mapping = mapping,
                FileName = fileName
            }).Output;
        public static IStream<IFileValue> ToExcelFile<TIn>(
            this IStream<TIn> stream,
            string name,
            string fileName,
            Func<ExcelFileArgBuilder, ExcelFileDefinition<TIn>> mapBuilder = null)
            => new ToExcelFileStreamNode<TIn>(name, new ToExcelFileArgs<TIn>
            {
                MainStream = stream,
                Mapping = mapBuilder == null ? null : mapBuilder(new()),
                FileName = fileName
            }).Output;
        #endregion
    }
}
