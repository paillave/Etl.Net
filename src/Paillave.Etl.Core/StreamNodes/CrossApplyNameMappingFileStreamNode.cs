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
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.StreamNodes
{
    //public class CrossApplyNameMappingTextFileArgs<TIn, TParsed, TOut> where TParsed : new()
    //{
    //    public ColumnNameFlatFileDescriptor<TParsed> Mapping { get; set; }
    //    public Func<TIn, TParsed, TOut> ResultSelector { get; set; }
    //    public Func<TIn, Stream> DataStreamSelector { get; set; }
    //    public bool NoParallelisation { get; set; } = false;
    //}

    //public class CrossApplyNameMappingTextFileStreamNode<TIn, TParsed, TOut> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyNameMappingTextFileArgs<TIn, TParsed, TOut>>, IStreamNodeOutput<TOut> where TParsed : new()
    //{
    //    private Semaphore _sem;
    //    public CrossApplyNameMappingTextFileStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyNameMappingTextFileArgs<TIn, TParsed, TOut> args) : base(input, name, parentNodeNamePath, args)
    //    {
    //        _sem = args.NoParallelisation ? new Semaphore(1, 1) : new Semaphore(10, 10);
    //        this.Output = base.CreateStream(nameof(this.Output), input.Observable.FlatMap(i => CreateOutputObservable(i, args)));
    //    }

    //    private IPushObservable<TOut> CreateOutputObservable(TIn input, CrossApplyNameMappingTextFileArgs<TIn, TParsed, TOut> args)
    //    {
    //        var src = new DeferedPushObservable<string>(pushValue =>
    //         {
    //             _sem.WaitOne();
    //             using (var sr = new StreamReader(args.DataStreamSelector(input)))
    //                 while (!sr.EndOfStream)
    //                     pushValue(sr.ReadLine());
    //             _sem.Release();
    //         });
    //        var splittedLineS = src.Map(args.Mapping.LineSplitter);
    //        var lineParserS = splittedLineS
    //            .Skip(args.Mapping.LinesToIgnore)
    //            .Take(1)
    //            .Map(args.Mapping.ColumnNameMappingConfiguration.LineParser);
    //        var dataLineS = splittedLineS
    //            .Skip(1 + args.Mapping.LinesToIgnore)
    //            .Filter(i => i.Count > 0);
    //        var ret = dataLineS.CombineWithLatest(lineParserS, (dataLine, lineParser) => args.ResultSelector(input, lineParser(dataLine)));
    //        return new DeferedWrapperPushObservable<TOut>(ret, src.Start);
    //    }

    //    public IStream<TOut> Output { get; }
    //}

    public static partial class StreamEx
    {
    }
}