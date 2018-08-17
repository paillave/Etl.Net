using Paillave.Etl.Core;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.StreamNodes;
using Paillave.RxPush.Core;

namespace Paillave.Etl.StreamNodes
{
    public class CrossApplyDataStreamReadArgs<TIn, TOut>
    {
        public Func<TIn, Stream> GetStream { get; set; }
        public Func<TIn, string, TOut> Selector { get; set; }
    }
    public class CrossApplyDataStreamReadNode<TIn, TOut> : StreamNodeBase<IStream<TIn>, TIn, CrossApplyDataStreamReadArgs<TIn, TOut>>, IStreamNodeOutput<TOut>
    {
        public CrossApplyDataStreamReadNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, CrossApplyDataStreamReadArgs<TIn, TOut> args) : base(input, name, parentNodeNamePath, args)
        {
            this.Output = this.CreateStream(nameof(Output), input.Observable.FlatMap(s =>
            {
                var srcPushObservable = new DeferedPushObservable<string>(pushValue => this.ReadStream(args.GetStream(s), pushValue));
                return new DeferedWrapperPushObservable<TOut>(srcPushObservable.Map(str => args.Selector(s, str)), srcPushObservable.Start);
            }));
        }

        public IStream<TOut> Output { get; }

        private void ReadStream(Stream inStream, Action<string> pushValue)
        {
            using (var sr = new StreamReader(inStream))
                while (!sr.EndOfStream)
                    pushValue(sr.ReadLine());
        }
    }

    //public static partial class StreamEx
    //{
    //    public static IStream<TOut> CrossApplyDataStream<TOut>(this IStream<Stream> stream, string name, Func<string, TOut> resultSelector)
    //    {
    //        return new CrossApplyDataStreamReadNode<Stream, TOut>(stream, name, null, new CrossApplyDataStreamReadArgs<Stream, TOut>
    //        {
    //            GetStream = s => s,
    //            Selector = (_, s) => resultSelector(s)
    //        }).Output;
    //    }
    //    public static IStream<TOut> CrossApplyDataStream<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, Stream> getStream, Func<TIn, string, TOut> resultSelector)
    //    {
    //        return new CrossApplyDataStreamReadNode<TIn, TOut>(stream, name, null, new CrossApplyDataStreamReadArgs<TIn, TOut>
    //        {
    //            GetStream = getStream,
    //            Selector = resultSelector
    //        }).Output;
    //    }
    //    public static IStream<string> CrossApplyDataStream<TIn>(this IStream<TIn> stream, string name, Func<TIn, Stream> getStream)
    //    {
    //        return new CrossApplyDataStreamReadNode<TIn, string>(stream, name, null, new CrossApplyDataStreamReadArgs<TIn, string>
    //        {
    //            GetStream = getStream,
    //            Selector = (_, s) => s
    //        }).Output;
    //    }
    //    public static IStream<string> CrossApplyDataStream(this IStream<Stream> stream, string name)
    //    {
    //        return new CrossApplyDataStreamReadNode<Stream, string>(stream, name, null, new CrossApplyDataStreamReadArgs<Stream, string>
    //        {
    //            GetStream = s => s,
    //            Selector = (_, s) => s
    //        }).Output;
    //    }
    //    //public static NodeOutputError<TOut, TIn1, TIn2> CombineLatestKeepErrors<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
    //    //{
    //    //    var ret = new CombineLatestStreamNode<TIn1, TIn2, TOut>(stream, name, null, new CombineLatestArgs<TIn1, TIn2, TOut>
    //    //    {
    //    //        InputStream2 = inputStream2,
    //    //        ResultSelector = resultSelector,
    //    //        RedirectErrorsInsteadOfFail = true
    //    //    });
    //    //    return new NodeOutputError<TOut, TIn1, TIn2>(ret.Output, ret.Error);
    //    //}
    //}
}