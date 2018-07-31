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

namespace Paillave.Etl.Core.StreamNodes
{
    public abstract class FlatFileDescriptorBase<T> where T : new()
    {
        //internal CultureInfo CultureInfo { get; private set; } = null;
        internal int LinesToIgnore { get; private set; } = 0;
        internal Expression<Func<T, string>> FileNameLambda { get; private set; } = null;
        internal Func<string, IList<string>> LineSplitter { get; private set; } = Mappers.CsvLineSplitter();

        public void IsFieldDelimited(char fieldDelimiter = ';', char textDelimiter = '"')
        {
            LineSplitter = Mappers.CsvLineSplitter(fieldDelimiter, textDelimiter);
        }
        public void IgnoreFirstLines(int linesToIgnore)
        {
            LinesToIgnore = linesToIgnore;
        }
        public void MapFileNameToProperty(Expression<Func<T, string>> fileNameLambda)
        {
            FileNameLambda = fileNameLambda;
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

    public class CrossApplyNameMappingParsedFileStreamNode<TOut> : StreamNodeBase<IStream<string>, string, ColumnNameFlatFileDescriptor<TOut>>, IStreamNodeOutput<TOut> where TOut : new()
    {
        public CrossApplyNameMappingParsedFileStreamNode(IStream<string> input, string name, IEnumerable<string> parentNodeNamePath, ColumnNameFlatFileDescriptor<TOut> args) : base(input, name, parentNodeNamePath, args)
        {
            string parentNodeName = name;
            var splittedLineS = input
                .Select($"{parentNodeName}.Open File", i => (Stream)File.OpenRead(i))
                .CrossApplyDataStream($"{parentNodeName}.Read File")
                .Select($"{parentNodeName}.Split Lines", args.LineSplitter);

            var lineParserS = splittedLineS
                .Skip("skip lines to ignore", args.LinesToIgnore)
                .Top($"{parentNodeName}.take first header line only", 1)
                .Select($"{parentNodeName}.create line processor", args.ColumnNameMappingConfiguration.LineParser);
            var dataLineS = splittedLineS.Skip($"{parentNodeName}.take everything after the first line", 1 + args.LinesToIgnore);
            this.Output = dataLineS.CombineLatest($"{parentNodeName}.parse every line", lineParserS, (dataLine, lineParser) => lineParser(dataLine));
        }

        public IStream<TOut> Output { get; }
    }

    public class CrossApplyIndexMappingParsedFileStreamNode<TOut> : StreamNodeBase<IStream<string>, string, ColumnIndexFlatFileDescriptor<TOut>>, IStreamNodeOutput<TOut> where TOut : new()
    {
        public CrossApplyIndexMappingParsedFileStreamNode(IStream<string> input, string name, IEnumerable<string> parentNodeNamePath, ColumnIndexFlatFileDescriptor<TOut> args) : base(input, name, parentNodeNamePath, args)
        {
            string parentNodeName = name;
            var splittedLineS = input
                .Select($"{parentNodeName}.Open File", i => (Stream)File.OpenRead(i))
                .CrossApplyDataStream($"{parentNodeName}.Read File")
                .Select($"{parentNodeName}.Split Lines", args.LineSplitter);

            var dataLineS = splittedLineS.Skip($"{parentNodeName}.take out lines to ignore", args.LinesToIgnore);
            var lineParser = args.ColumnIndexMappingConfiguration.LineParser();
            this.Output = dataLineS.Select($"{parentNodeName}.parse every line", dataLine => lineParser(dataLine));
        }

        public IStream<TOut> Output { get; }
    }

    public static partial class StreamEx
    {
        public static IStream<TOut> CrossApplyParsedFile<TOut>(this IStream<string> stream, string name, ColumnNameFlatFileDescriptor<TOut> args)
            where TOut : new()
        {
            return new CrossApplyNameMappingParsedFileStreamNode<TOut>(stream, name, null, args).Output;
        }
        public static IStream<TOut> CrossApplyColumnNameParsedFile<TOut, TFileDesc>(this IStream<string> stream, string name)
            where TOut : new()
            where TFileDesc : ColumnNameFlatFileDescriptor<TOut>, new()
        {
            return new CrossApplyNameMappingParsedFileStreamNode<TOut>(stream, name, null, new TFileDesc()).Output;
        }
        public static IStream<TOut> CrossApplyParsedFile<TOut>(this IStream<string> stream, string name, ColumnIndexFlatFileDescriptor<TOut> args) 
            where TOut : new()
        {
            return new CrossApplyIndexMappingParsedFileStreamNode<TOut>(stream, name, null, args).Output;
        }
        public static IStream<TOut> CrossApplyColumnIndexParsedFile<TOut, TFileDesc>(this IStream<string> stream, string name)
            where TOut : new()
            where TFileDesc : ColumnIndexFlatFileDescriptor<TOut>, new()
        {
            return new CrossApplyIndexMappingParsedFileStreamNode<TOut>(stream, name, null, new TFileDesc()).Output;
        }
    }
}