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

namespace Paillave.Etl.Core.StreamNodes
{
    public class CrossApplyParsedFileArgs<TOut>
    {
        public Func<string, IList<string>> LineSplitter { get; }
        public ColumnIndexMappingConfiguration<TOut> ColumnIndexMapping { get; } = null;
        public ColumnNameMappingConfiguration<TOut> ColumnNameMapping { get; } = null;
        public int HeaderLinesToIgnore { get; } = 0;
        public CrossApplyParsedFileArgs(ColumnIndexMappingConfiguration<TOut> columnIndexMapping, int headerLinesToIgnore, char fieldDemimiter = ';', char textDemimiter = '"')
        {
            HeaderLinesToIgnore = headerLinesToIgnore;
            ColumnIndexMapping = columnIndexMapping;
            LineSplitter = Mappers.CsvLineSplitter(fieldDemimiter, textDemimiter);
        }
        public CrossApplyParsedFileArgs(ColumnIndexMappingConfiguration<TOut> columnIndexMapping, int headerLinesToIgnore, params int[] columnSize)
        {
            HeaderLinesToIgnore = headerLinesToIgnore;
            ColumnIndexMapping = columnIndexMapping;
            LineSplitter = Mappers.FixedColumnLineSplitter(columnSize);
        }
        public CrossApplyParsedFileArgs(ColumnNameMappingConfiguration<TOut> columnNameMapping, char fieldDemimiter = ';', char textDemimiter = '"')
        {
            ColumnNameMapping = columnNameMapping;
            LineSplitter = Mappers.CsvLineSplitter(fieldDemimiter, textDemimiter);
        }
        public CrossApplyParsedFileArgs(ColumnNameMappingConfiguration<TOut> columnNameMapping, params int[] columnSize)
        {
            ColumnNameMapping = columnNameMapping;
            LineSplitter = Mappers.FixedColumnLineSplitter(columnSize);
        }
    }

    public class CrossApplyParsedFileStreamNode<TOut> : StreamNodeBase<IStream<string>, string, CrossApplyParsedFileArgs<TOut>>, IStreamNodeOutput<TOut>
    {
        public CrossApplyParsedFileStreamNode(IStream<string> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyParsedFileArgs<TOut> args) : base(input, name, parentNodeNamePath, args)
        {
            string parentNodeName = name;
            var splittedLineS = input
                .Select($"{parentNodeName}.Open File", i => (Stream)File.OpenRead(i))
                .CrossApplyDataStream($"{parentNodeName}.Read File")
                .Select($"{parentNodeName}.Split Lines", args.LineSplitter);

            if (args.ColumnNameMapping != null)
            {
                var lineParserS = splittedLineS
                    .Top($"{parentNodeName}.take first header line only", 1)
                    .Select($"{parentNodeName}.create line processor", args.ColumnNameMapping.LineParser);
                var dataLineS = splittedLineS.Skip($"{parentNodeName}.take everything after the first line", 1);
                this.Output = dataLineS.CombineLatest($"{parentNodeName}.parse every line", lineParserS, (dataLine, lineParser) => lineParser(dataLine));
            }
            else
            {
                var dataLineS = splittedLineS.Skip($"{parentNodeName}.take out the header", args.HeaderLinesToIgnore);
                var lineParser = args.ColumnIndexMapping.LineParser();
                this.Output = dataLineS.Select($"{parentNodeName}.parse every line", dataLine => lineParser(dataLine));
            }
        }

        public IStream<TOut> Output { get; }

        //private void ReadStream(Stream inStream, Action<string> pushValue)
        //{
        //    using (var sr = new StreamReader(inStream))
        //        while (!sr.EndOfStream)
        //            pushValue(sr.ReadLine());
        //}
    }

    public static partial class StreamEx
    {
        public static IStream<TOut> CrossApplyParsedFile<TOut>(this IStream<string> stream, string name, CrossApplyParsedFileArgs<TOut> args)
        {
            return new CrossApplyParsedFileStreamNode<TOut>(stream, name, null, args).Output;
        }
    }
}