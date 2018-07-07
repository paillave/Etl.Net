using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class FilterStreamNode<TIn> : StreamNodeBase, IStreamNodeOutput<TIn>, IStreamNodeError<ErrorRow<TIn>>
    {
        public FilterStreamNode(IStream<TIn> inputStream, string name, Func<TIn, bool> predicate, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream.Observable.Select(base.ErrorManagementWrapFunction(predicate));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Where(i => i.Output).Select(i => i.Input));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TIn>(i.Input, i.Exception)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), inputStream.Observable.Where(predicate));
        }
        public IStream<TIn> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }

    public class FilterSortedStreamNode<TIn> : StreamNodeBase, ISortedStreamNodeOutput<TIn>, IStreamNodeError<ErrorRow<TIn>>
    {
        public FilterSortedStreamNode(ISortedStream<TIn> inputStream, string name, Func<TIn, bool> predicate, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream.Observable.Select(base.ErrorManagementWrapFunction(predicate));
                this.Output = base.CreateSortedStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Where(i => i.Output).Select(i => i.Input), inputStream.SortCriterias);
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TIn>(i.Input, i.Exception)));
            }
            else
                this.Output = base.CreateSortedStream(nameof(this.Output), inputStream.Observable.Where(predicate), inputStream.SortCriterias);
        }
        public ISortedStream<TIn> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }

    public static partial class StreamEx
    {
        public static ISortedStream<TIn> Filter<TIn>(this ISortedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new FilterSortedStreamNode<TIn>(stream, name, predicate, false).Output;
        }

        public static SortedNodeOutputError<TIn, TIn> FilterKeepErrors<TIn>(this ISortedStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new FilterSortedStreamNode<TIn>(stream, name, predicate, true);
            return new SortedNodeOutputError<TIn, TIn>(ret.Output, ret.Error);
        }

        public static IStream<TIn> Filter<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            return new FilterStreamNode<TIn>(stream, name, predicate, false).Output;
        }

        public static NodeOutputError<TIn, TIn> FilterKeepErrors<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate)
        {
            var ret = new FilterStreamNode<TIn>(stream, name, predicate, true);
            return new NodeOutputError<TIn, TIn>(ret.Output, ret.Error);
        }
    }
}
