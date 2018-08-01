using Paillave.Etl.Core.System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.System.Streams;
using Paillave.Etl.Core.MapperFactories;
using Paillave.Etl.Core.Helpers.MapperFactories;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Paillave.RxPush.Core;

namespace Paillave.Etl.Core.StreamNodes
{
    public abstract class FlatFileDescriptorBase<T> where T : new()
    {
        internal int LinesToIgnore { get; private set; } = 0;

        internal Func<string, IList<string>> LineSplitter { get; private set; } = Mappers.CsvLineSplitter();

        public void IsFieldDelimited(char fieldDelimiter = ';', char textDelimiter = '"')
        {
            LineSplitter = Mappers.CsvLineSplitter(fieldDelimiter, textDelimiter);
        }
        public void IgnoreFirstLines(int linesToIgnore)
        {
            LinesToIgnore = linesToIgnore;
        }
    }

    public class ColumnNameFlatFileDescriptor<T> : FlatFileDescriptorBase<T> where T : new()
    {
        internal ColumnNameMappingConfiguration<T> ColumnNameMappingConfiguration { get; private set; }

        public ColumnNameFlatFileDescriptor()
        {
            this.ColumnNameMappingConfiguration = new ColumnNameMappingConfiguration<T>(() => new T());
        }
        public void MapColumnToProperty<TField>(string columnName, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            this.ColumnNameMappingConfiguration.MapColumnToProperty(columnName, memberLamda, cultureInfo);
        }
        public void WithCultureInfo(CultureInfo cultureInfo)
        {
            this.ColumnNameMappingConfiguration.WithCultureInfo(cultureInfo);
        }
    }

    public class ColumnIndexFlatFileDescriptor<T> : FlatFileDescriptorBase<T> where T : new()
    {
        internal ColumnIndexMappingConfiguration<T> ColumnIndexMappingConfiguration { get; private set; }

        public ColumnIndexFlatFileDescriptor()
        {
            this.ColumnIndexMappingConfiguration = new ColumnIndexMappingConfiguration<T>(() => new T());
        }
        public void MapColumnToProperty<TField>(int index, Expression<Func<T, TField>> memberLamda, CultureInfo cultureInfo = null)
        {
            this.ColumnIndexMappingConfiguration.MapColumnToProperty(index, memberLamda, cultureInfo);
        }
        public void WithCultureInfo(CultureInfo cultureInfo)
        {
            this.ColumnIndexMappingConfiguration.WithCultureInfo(cultureInfo);
        }
    }

    public class CrossApplyNameMappingParsedFileArgs<TIn, TParsed, TOut> where TParsed : new()
    {
        public ColumnNameFlatFileDescriptor<TParsed> Mapping { get; set; }
        public Func<TIn, TParsed, TOut> ResultSelector { get; set; }
        public Func<TIn, string> FilePathSelector { get; set; }
    }

    public class CrossApplyNameMappingParsedFileStreamNode<TIn, TParsed, TOut> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyNameMappingParsedFileArgs<TIn, TParsed, TOut>>, IStreamNodeOutput<TOut> where TParsed : new()
    {
        public CrossApplyNameMappingParsedFileStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyNameMappingParsedFileArgs<TIn, TParsed, TOut> args) : base(input, name, parentNodeNamePath, args)
        {
            this.Output = base.CreateStream(nameof(this.Output), input.Observable.FlatMap(i => CreateOutputObservable(i, args)));
        }

        private IPushObservable<TOut> CreateOutputObservable(TIn input, CrossApplyNameMappingParsedFileArgs<TIn, TParsed, TOut> args)
        {
            var splittedLineS = new DeferedPushObservable<string>(pushValue =>
              {
                  using (var sr = new StreamReader(File.OpenRead(args.FilePathSelector(input))))
                      while (!sr.EndOfStream)
                          pushValue(sr.ReadLine());
              }, true).Map(args.Mapping.LineSplitter);
            var lineParserS = splittedLineS
                .Skip(args.Mapping.LinesToIgnore)
                .Take(1)
                .Map(args.Mapping.ColumnNameMappingConfiguration.LineParser);
            var dataLineS = splittedLineS.Skip(1 + args.Mapping.LinesToIgnore);

            return dataLineS.CombineWithLatest(lineParserS, (dataLine, lineParser) => args.ResultSelector(input, lineParser(dataLine)));
        }

        public IStream<TOut> Output { get; }
    }

    public class CrossApplyIndexMappingParsedFileArgs<TIn, TParsed, TOut> where TParsed : new()
    {
        public ColumnIndexFlatFileDescriptor<TParsed> Mapping { get; set; }
        public Func<TIn, TParsed, TOut> ResultSelector { get; set; }
        public Func<TIn, string> FilePathSelector { get; set; }
    }

    public class CrossApplyIndexMappingParsedFileStreamNode<TIn, TParsed, TOut> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyIndexMappingParsedFileArgs<TIn, TParsed, TOut>>, IStreamNodeOutput<TOut> where TParsed : new()
    {
        public CrossApplyIndexMappingParsedFileStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyIndexMappingParsedFileArgs<TIn, TParsed, TOut> args) : base(input, name, parentNodeNamePath, args)
        {
            this.Output = base.CreateStream(nameof(this.Output), input.Observable.FlatMap(i => CreateOutputObservable(i, args)));
        }
        private IPushObservable<TOut> CreateOutputObservable(TIn input, CrossApplyIndexMappingParsedFileArgs<TIn, TParsed, TOut> args)
        {
            var splittedLineS = new DeferedPushObservable<string>(pushValue =>
            {
                using (var sr = new StreamReader(File.OpenRead(args.FilePathSelector(input))))
                    while (!sr.EndOfStream)
                        pushValue(sr.ReadLine());
            }, true).Map(args.Mapping.LineSplitter);

            var dataLineS = splittedLineS.Skip(args.Mapping.LinesToIgnore);
            var inputLineParser = args.Mapping.ColumnIndexMappingConfiguration.LineParser();
            return dataLineS.Map(dataLine => args.ResultSelector(input, inputLineParser(dataLine)));
        }
        public IStream<TOut> Output { get; }
    }

    public static partial class StreamEx
    {
        public static IStream<TOut> CrossApplyParsedFile<TOut>(this IStream<string> stream, string name, ColumnNameFlatFileDescriptor<TOut> args)
            where TOut : new()
        {
            return new CrossApplyNameMappingParsedFileStreamNode<string, TOut, TOut>(stream, name, null, new CrossApplyNameMappingParsedFileArgs<string, TOut, TOut>
            {
                FilePathSelector = i => i,
                ResultSelector = (i, j) => j,
                Mapping = args
            }).Output;
        }
        public static IStream<TOut> CrossApplyParsedFile<TIn, TOut>(this IStream<TIn> stream, string name, ColumnNameFlatFileDescriptor<TOut> args, Func<TIn, string> filePathSelector)
            where TOut : new()
        {
            return new CrossApplyNameMappingParsedFileStreamNode<TIn, TOut, TOut>(stream, name, null, new CrossApplyNameMappingParsedFileArgs<TIn, TOut, TOut>
            {
                FilePathSelector = filePathSelector,
                ResultSelector = (i, j) => j,
                Mapping = args
            }).Output;
        }
        public static IStream<TOut> CrossApplyParsedFile<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, ColumnNameFlatFileDescriptor<TParsed> args, Func<TIn, string> filePathSelector, Func<TIn, TParsed, TOut> resultSelector) where TParsed : new()
        {
            return new CrossApplyNameMappingParsedFileStreamNode<TIn, TParsed, TOut>(stream, name, null, new CrossApplyNameMappingParsedFileArgs<TIn, TParsed, TOut>
            {
                FilePathSelector = filePathSelector,
                ResultSelector = resultSelector,
                Mapping = args
            }).Output;
        }
        public static IStream<TOut> CrossApplyParsedFile<TParsed, TOut>(this IStream<string> stream, string name, ColumnNameFlatFileDescriptor<TParsed> args, Func<string, TParsed, TOut> resultSelector) where TParsed : new()
        {
            return new CrossApplyNameMappingParsedFileStreamNode<string, TParsed, TOut>(stream, name, null, new CrossApplyNameMappingParsedFileArgs<string, TParsed, TOut>
            {
                FilePathSelector = i => i,
                ResultSelector = resultSelector,
                Mapping = args
            }).Output;
        }



        public static IStream<TOut> CrossApplyParsedFile<TOut>(this IStream<string> stream, string name, ColumnIndexFlatFileDescriptor<TOut> args)
            where TOut : new()
        {
            return new CrossApplyIndexMappingParsedFileStreamNode<string, TOut, TOut>(stream, name, null, new CrossApplyIndexMappingParsedFileArgs<string, TOut, TOut>
            {
                FilePathSelector = i => i,
                ResultSelector = (i, j) => j,
                Mapping = args
            }).Output;
        }
        public static IStream<TOut> CrossApplyParsedFile<TIn, TOut>(this IStream<TIn> stream, string name, ColumnIndexFlatFileDescriptor<TOut> args, Func<TIn, string> filePathSelector)
            where TOut : new()
        {
            return new CrossApplyIndexMappingParsedFileStreamNode<TIn, TOut, TOut>(stream, name, null, new CrossApplyIndexMappingParsedFileArgs<TIn, TOut, TOut>
            {
                FilePathSelector = filePathSelector,
                ResultSelector = (i, j) => j,
                Mapping = args
            }).Output;
        }
        public static IStream<TOut> CrossApplyParsedFile<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, ColumnIndexFlatFileDescriptor<TParsed> args, Func<TIn, string> filePathSelector, Func<TIn, TParsed, TOut> resultSelector) where TParsed : new()
        {
            return new CrossApplyIndexMappingParsedFileStreamNode<TIn, TParsed, TOut>(stream, name, null, new CrossApplyIndexMappingParsedFileArgs<TIn, TParsed, TOut>
            {
                FilePathSelector = filePathSelector,
                ResultSelector = resultSelector,
                Mapping = args
            }).Output;
        }
        public static IStream<TOut> CrossApplyParsedFile<TParsed, TOut>(this IStream<string> stream, string name, ColumnIndexFlatFileDescriptor<TParsed> args, Func<string, TParsed, TOut> resultSelector) where TParsed : new()
        {
            return new CrossApplyIndexMappingParsedFileStreamNode<string, TParsed, TOut>(stream, name, null, new CrossApplyIndexMappingParsedFileArgs<string, TParsed, TOut>
            {
                FilePathSelector = i => i,
                ResultSelector = resultSelector,
                Mapping = args
            }).Output;
        }
    }
}