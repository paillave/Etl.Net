using Paillave.Etl.Core;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using System.Reflection;
using Paillave.RxPush.Core;
using System.Threading;
using Paillave.Etl.Helpers;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{

    public class CrossApplyNameMappingTextFileArgs<TIn, TParsed, TOut> where TParsed : new()
    {
        public ColumnNameFlatFileDescriptor<TParsed> Mapping { get; set; }
        public Func<TIn, TParsed, TOut> ResultSelector { get; set; }
        public Func<TIn, Stream> DataStreamSelector { get; set; }
        public bool NoParallelisation { get; set; } = false;
    }

    public class CrossApplyNameMappingTextFileStreamNode<TIn, TParsed, TOut> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyNameMappingTextFileArgs<TIn, TParsed, TOut>>, IStreamNodeOutput<TOut> where TParsed : new()
    {
        private Semaphore _sem;
        public CrossApplyNameMappingTextFileStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyNameMappingTextFileArgs<TIn, TParsed, TOut> args) : base(input, name, parentNodeNamePath, args)
        {
            _sem = args.NoParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
            this.Output = base.CreateStream(nameof(this.Output), input.Observable.FlatMap(i => CreateOutputObservable(i, args)));
        }

        private IPushObservable<TOut> CreateOutputObservable(TIn input, CrossApplyNameMappingTextFileArgs<TIn, TParsed, TOut> args)
        {
            var splittedLineS = new DeferedPushObservable<string>(pushValue =>
              {
                  _sem.WaitOne();
                  using (var sr = new StreamReader(args.DataStreamSelector(input)))
                      while (!sr.EndOfStream)
                          pushValue(sr.ReadLine());
                  _sem.Release();
              }, true).Map(args.Mapping.LineSplitter);
            var lineParserS = splittedLineS
                .Skip(args.Mapping.LinesToIgnore)
                .Take(1)
                .Map(args.Mapping.ColumnNameMappingConfiguration.LineParser);
            var dataLineS = splittedLineS
                .Skip(1 + args.Mapping.LinesToIgnore)
                .Filter(i => i.Count > 0);

            return dataLineS.CombineWithLatest(lineParserS, (dataLine, lineParser) => args.ResultSelector(input, lineParser(dataLine)));
        }

        public IStream<TOut> Output { get; }
    }

    public static partial class StreamEx
    {
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<string> stream, string name, ColumnNameFlatFileDescriptor<TOut> args, bool noParallelisation = false)
            where TOut : new()
        {
            return new CrossApplyNameMappingTextFileStreamNode<string, TOut, TOut>(stream, name, null, new CrossApplyNameMappingTextFileArgs<string, TOut, TOut>
            {
                DataStreamSelector = i => File.OpenRead(i),
                ResultSelector = (i, j) => j,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<Stream> stream, string name, ColumnNameFlatFileDescriptor<TOut> args, bool noParallelisation = false)
            where TOut : new()
        {
            return new CrossApplyNameMappingTextFileStreamNode<Stream, TOut, TOut>(stream, name, null, new CrossApplyNameMappingTextFileArgs<Stream, TOut, TOut>
            {
                DataStreamSelector = i => i,
                ResultSelector = (i, j) => j,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TOut>(this IStream<TIn> stream, string name, ColumnNameFlatFileDescriptor<TOut> args, Func<TIn, string> filePathSelector, bool noParallelisation = false)
            where TOut : new()
        {
            return new CrossApplyNameMappingTextFileStreamNode<TIn, TOut, TOut>(stream, name, null, new CrossApplyNameMappingTextFileArgs<TIn, TOut, TOut>
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                ResultSelector = (i, j) => j,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, ColumnNameFlatFileDescriptor<TParsed> args, Func<TIn, string> filePathSelector, Func<TIn, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return new CrossApplyNameMappingTextFileStreamNode<TIn, TParsed, TOut>(stream, name, null, new CrossApplyNameMappingTextFileArgs<TIn, TParsed, TOut>
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                ResultSelector = resultSelector,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<string> stream, string name, ColumnNameFlatFileDescriptor<TParsed> args, Func<string, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return new CrossApplyNameMappingTextFileStreamNode<string, TParsed, TOut>(stream, name, null, new CrossApplyNameMappingTextFileArgs<string, TParsed, TOut>
            {
                DataStreamSelector = i => File.OpenRead(i),
                ResultSelector = resultSelector,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<Stream> stream, string name, ColumnNameFlatFileDescriptor<TParsed> args, Func<Stream, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return new CrossApplyNameMappingTextFileStreamNode<Stream, TParsed, TOut>(stream, name, null, new CrossApplyNameMappingTextFileArgs<Stream, TParsed, TOut>
            {
                DataStreamSelector = i => i,
                ResultSelector = resultSelector,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
    }
}