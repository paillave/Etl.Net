using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.System.Streams;
using Paillave.Etl.Core.System.NodeOutputs;

namespace Paillave.Etl.Core.StreamNodes
{
    public class CombineLatestArgs<TIn1, TIn2, TOut>
    {
        public IStream<TIn2> InputStream2 { get; set; }
        public Func<TIn1, TIn2, TOut> ResultSelector { get; set; }
        public bool RedirectErrorsInsteadOfFail { get; set; }
    }
    public class CombineLatestStreamNode<TIn1, TIn2, TOut> : StreamNodeBase<IStream<TIn1>, TIn1, CombineLatestArgs<TIn1, TIn2, TOut>>, IStreamNodeError<ErrorRow<TIn1, TIn2>>, IStreamNodeOutput<TOut>
    {
        public CombineLatestStreamNode(IStream<TIn1> inputStream1, string name, IEnumerable<string> parentNodeNamePath, CombineLatestArgs<TIn1, TIn2, TOut> args)
            : base(inputStream1, name, parentNodeNamePath, args)
        {
            //base.Initialize(inputStream1.ExecutionContext ?? inputStream2.ExecutionContext, name, parentNodeNamePath);
            if (args.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream1.Observable.CombineWithLatest(args.InputStream2.Observable, base.ErrorManagementWrapFunction(args.ResultSelector));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Map(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TIn1, TIn2>(i)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), inputStream1.Observable.CombineWithLatest(args.InputStream2.Observable, args.ResultSelector));
        }
        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn1, TIn2>> Error { get; }
    }
    public static partial class StreamEx
    {
        public static IStream<TOut> CombineLatest<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
        {
            return new CombineLatestStreamNode<TIn1, TIn2, TOut>(stream, name, null, new CombineLatestArgs<TIn1, TIn2, TOut>
            {
                InputStream2 = inputStream2,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static INodeOutputError<TOut, TIn1, TIn2> CombineLatestKeepErrors<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
        {
            var ret = new CombineLatestStreamNode<TIn1, TIn2, TOut>(stream, name, null, new CombineLatestArgs<TIn1, TIn2, TOut>
            {
                InputStream2 = inputStream2,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<CombineLatestStreamNode<TIn1, TIn2, TOut>, TOut, TIn1, TIn2>(ret);
        }
    }
}
