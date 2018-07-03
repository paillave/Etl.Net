using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class MapStreamNode<TIn, TOut> : StreamNodeBase, IStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn>>
    {
        public MapStreamNode(IStream<TIn> inputStream, string name, Func<TIn, TOut> mapper, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream.Observable.Select(base.ErrorManagementWrapFunction(mapper));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TIn>(i.Input, i.Exception)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), inputStream.Observable.Select(mapper));
        }
        public MapStreamNode(IStream<TIn> inputStream, string name, Func<TIn, int, TOut> mapper, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream.Observable.Select(base.ErrorManagementWrapFunction(mapper));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TIn>(i.Input, i.Exception)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), inputStream.Observable.Select(mapper));
        }
        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
    public static partial class StreamEx
    {
        public static IStream<TOut> Map<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper)
        {
            return new MapStreamNode<TIn, TOut>(stream, name, mapper, false).Output;
        }
        public static IStream<TOut> Map<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, TOut> mapper)
        {
            return new MapStreamNode<TIn, TOut>(stream, name, mapper, false).Output;
        }
        public static NodeOutputError<TOut, TIn> MapKeepErrors<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, TOut> mapper)
        {
            var ret = new MapStreamNode<TIn, TOut>(stream, name, mapper, true);
            return new NodeOutputError<TOut, TIn>(ret.Output, ret.Error);
        }
        public static NodeOutputError<TOut, TIn> MapKeepErrors<TIn, TOut>(this IStream<TIn> stream, string name, Func<TIn, int, TOut> mapper)
        {
            var ret = new MapStreamNode<TIn, TOut>(stream, name, mapper, true);
            return new NodeOutputError<TOut, TIn>(ret.Output, ret.Error);
        }
    }
}
