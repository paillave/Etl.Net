using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class TopStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, int>, IStreamNodeOutput<TIn>
    {
        public IStream<TIn> Output { get; }
        public TopStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, int arguments) : base(input, name, parentNodeNamePath, arguments)
        {
            this.Output = base.CreateStream(nameof(Output), input.Observable.Take(arguments));
        }
    }
    public class TopSortedStreamNode<TIn> : StreamNodeBase<ISortedStream<TIn>, TIn, int>, ISortedStreamNodeOutput<TIn>
    {
        public ISortedStream<TIn> Output { get; }
        public TopSortedStreamNode(ISortedStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, int arguments) : base(input, name, parentNodeNamePath, arguments)
        {
            this.Output = base.CreateSortedStream(nameof(Output), input.Observable.Take(arguments), input.SortCriterias);
        }
    }
    public class TopKeyedStreamNode<TIn> : StreamNodeBase<IKeyedStream<TIn>, TIn, int>, IKeyedStreamNodeOutput<TIn>
    {
        public IKeyedStream<TIn> Output { get; }
        public TopKeyedStreamNode(IKeyedStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, int arguments) : base(input, name, parentNodeNamePath, arguments)
        {
            this.Output = base.CreateKeyedStream(nameof(Output), input.Observable.Take(arguments), input.SortCriterias);
        }
    }
    public static partial class StreamEx
    {
        public static ISortedStream<TIn> Top<TIn>(this ISortedStream<TIn> stream, string name, int count)
        {
            return new TopSortedStreamNode<TIn>(stream, name, null, count).Output;
        }
        public static IKeyedStream<TIn> Top<TIn>(this IKeyedStream<TIn> stream, string name, int count)
        {
            return new TopKeyedStreamNode<TIn>(stream, name, null, count).Output;
        }
        public static IStream<TIn> Top<TIn>(this IStream<TIn> stream, string name, int count)
        {
            return new TopStreamNode<TIn>(stream, name, null, count).Output;
        }
    }
}
