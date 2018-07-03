using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class SkipStreamNode<TIn> : StreamNodeBase, IStreamNodeOutput<TIn>
    {
        public IStream<TIn> Output { get; }
        public SkipStreamNode(IStream<TIn> inputStream, string name, int count, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            this.Output = base.CreateStream(nameof(Output), inputStream.Observable.Skip(count));
        }
    }
    public class SkipSortedStreamNode<TIn> : StreamNodeBase, ISortedStreamNodeOutput<TIn>
    {
        public ISortedStream<TIn> Output { get; }
        public SkipSortedStreamNode(ISortedStream<TIn> inputStream, string name, int count, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            this.Output = base.CreateSortedStream(nameof(Output), inputStream.Observable.Skip(count), inputStream.SortCriterias);
        }
    }
    public class SkipKeyedStreamNode<TIn> : StreamNodeBase, IKeyedStreamNodeOutput<TIn>
    {
        public IKeyedStream<TIn> Output { get; }
        public SkipKeyedStreamNode(IKeyedStream<TIn> inputStream, string name, int count, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            this.Output = base.CreateKeyedStream(nameof(Output), inputStream.Observable.Skip(count), inputStream.SortCriterias);
        }
    }
    public static partial class StreamEx
    {
        public static IStream<TIn> Skip<TIn>(this IStream<TIn> stream, string name, int count)
        {
            return new SkipStreamNode<TIn>(stream, name, count).Output;
        }
        public static ISortedStream<TIn> Skip<TIn>(this ISortedStream<TIn> stream, string name, int count)
        {
            return new SkipSortedStreamNode<TIn>(stream, name, count).Output;
        }
        public static IKeyedStream<TIn> Skip<TIn>(this IKeyedStream<TIn> stream, string name, int count)
        {
            return new SkipKeyedStreamNode<TIn>(stream, name, count).Output;
        }
    }
}
