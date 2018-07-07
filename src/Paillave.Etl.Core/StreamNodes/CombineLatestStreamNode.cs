using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class CombineLatestStreamNode<TIn1, TIn2, TOut> : StreamNodeBase, IStreamNodeError<ErrorRow<TIn1, TIn2>>, IStreamNodeOutput<TOut>
    {
        public CombineLatestStreamNode(IStream<TIn1> inputStream1, string name, IEnumerable<string> parentNodeNamePath, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector, bool redirectErrorsInsteadOfFail)
        {
            base.Initialize(inputStream1.ExecutionContext ?? inputStream2.ExecutionContext, name, parentNodeNamePath);
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream1.Observable.CombineLatest(inputStream2.Observable, base.ErrorManagementWrapFunction(resultSelector));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TIn1, TIn2>(i.Input, i.Input2, i.Exception)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), inputStream1.Observable.CombineLatest(inputStream2.Observable, resultSelector));
        }
        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn1, TIn2>> Error { get; }
    }
    public static partial class StreamEx
    {
        public static IStream<TOut> CombineLatest<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
        {
            return new CombineLatestStreamNode<TIn1, TIn2, TOut>(stream, name, null, inputStream2, resultSelector, false).Output;
        }
        public static NodeOutputError<TOut, TIn1, TIn2> CombineLatestKeepErrors<TIn1, TIn2, TOut>(this IStream<TIn1> stream, string name, IStream<TIn2> inputStream2, Func<TIn1, TIn2, TOut> resultSelector)
        {
            var ret = new CombineLatestStreamNode<TIn1, TIn2, TOut>(stream, name, null, inputStream2, resultSelector, true);
            return new NodeOutputError<TOut, TIn1, TIn2>(ret.Output, ret.Error);
        }
    }
}
