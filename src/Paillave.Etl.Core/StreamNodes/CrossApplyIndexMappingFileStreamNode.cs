using Paillave.Etl.Core;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using System.Threading;
using Paillave.Etl.Helpers;
using System;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{
    public class CrossApplyIndexMappingTextFileArgs<TIn, TParsed, TOut> where TParsed : new()
    {
        public ColumnIndexFlatFileDescriptor<TParsed> Mapping { get; set; }
        public Func<TIn, TParsed, TOut> ResultSelector { get; set; }
        public Func<TIn, Stream> DataStreamSelector { get; set; }
        public bool NoParallelisation { get; set; } = false;
    }

    public class CrossApplyIndexMappingTextFileStreamNode<TIn, TParsed, TOut> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyIndexMappingTextFileArgs<TIn, TParsed, TOut>>, IStreamNodeOutput<TOut> where TParsed : new()
    {
        private Semaphore _sem;
        public CrossApplyIndexMappingTextFileStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyIndexMappingTextFileArgs<TIn, TParsed, TOut> args) : base(input, name, parentNodeNamePath, args)
        {
            _sem = args.NoParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
            this.Output = base.CreateStream(nameof(this.Output), input.Observable.FlatMap(i => CreateOutputObservable(i, args)));
        }
        private IPushObservable<TOut> CreateOutputObservable(TIn input, CrossApplyIndexMappingTextFileArgs<TIn, TParsed, TOut> args)
        {
            var splittedLineS = new DeferedPushObservable<string>(pushValue =>
            {
                _sem.WaitOne();
                using (var sr = new StreamReader(args.DataStreamSelector(input)))
                    while (!sr.EndOfStream)
                        pushValue(sr.ReadLine());
                _sem.Release();
            }, true).Map(args.Mapping.LineSplitter);

            var dataLineS = splittedLineS.Skip(args.Mapping.LinesToIgnore).Filter(i => i.Count > 0);
            var inputLineParser = args.Mapping.ColumnIndexMappingConfiguration.LineParser();
            return dataLineS.Map(dataLine => args.ResultSelector(input, inputLineParser(dataLine)));
        }
        public IStream<TOut> Output { get; }
    }

    public static partial class StreamEx
    {
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<string> stream, string name, ColumnIndexFlatFileDescriptor<TOut> args, bool noParallelisation = false)
            where TOut : new()
        {
            return new CrossApplyIndexMappingTextFileStreamNode<string, TOut, TOut>(stream, name, null, new CrossApplyIndexMappingTextFileArgs<string, TOut, TOut>
            {
                DataStreamSelector = i => File.OpenRead(i),
                ResultSelector = (i, j) => j,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TOut>(this IStream<Stream> stream, string name, ColumnIndexFlatFileDescriptor<TOut> args, bool noParallelisation = false)
            where TOut : new()
        {
            return new CrossApplyIndexMappingTextFileStreamNode<Stream, TOut, TOut>(stream, name, null, new CrossApplyIndexMappingTextFileArgs<Stream, TOut, TOut>
            {
                DataStreamSelector = i => i,
                ResultSelector = (i, j) => j,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TOut>(this IStream<TIn> stream, string name, ColumnIndexFlatFileDescriptor<TOut> args, Func<TIn, string> filePathSelector, bool noParallelisation = false)
            where TOut : new()
        {
            return new CrossApplyIndexMappingTextFileStreamNode<TIn, TOut, TOut>(stream, name, null, new CrossApplyIndexMappingTextFileArgs<TIn, TOut, TOut>
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                ResultSelector = (i, j) => j,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, ColumnIndexFlatFileDescriptor<TParsed> args, Func<TIn, string> filePathSelector, Func<TIn, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return new CrossApplyIndexMappingTextFileStreamNode<TIn, TParsed, TOut>(stream, name, null, new CrossApplyIndexMappingTextFileArgs<TIn, TParsed, TOut>
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                ResultSelector = resultSelector,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<string> stream, string name, ColumnIndexFlatFileDescriptor<TParsed> args, Func<string, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return new CrossApplyIndexMappingTextFileStreamNode<string, TParsed, TOut>(stream, name, null, new CrossApplyIndexMappingTextFileArgs<string, TParsed, TOut>
            {
                DataStreamSelector = i => File.OpenRead(i),
                ResultSelector = resultSelector,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
        public static IStream<TOut> CrossApplyTextFile<TParsed, TOut>(this IStream<Stream> stream, string name, ColumnIndexFlatFileDescriptor<TParsed> args, Func<Stream, TParsed, TOut> resultSelector, bool noParallelisation = false) where TParsed : new()
        {
            return new CrossApplyIndexMappingTextFileStreamNode<Stream, TParsed, TOut>(stream, name, null, new CrossApplyIndexMappingTextFileArgs<Stream, TParsed, TOut>
            {
                DataStreamSelector = i => i,
                ResultSelector = resultSelector,
                Mapping = args,
                NoParallelisation = noParallelisation
            }).Output;
        }
    }
}