using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class TakeStreamNode<TIn> : StreamNodeBase, IStreamNodeOutput<TIn>
    {
        public IStream<TIn> Output { get; }
        public TakeStreamNode(IStream<TIn> inputStream, string name, int count, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            this.Output = base.CreateStream(nameof(Output), inputStream.Observable.Take(count));
        }
    }
    public class TakeSortedStreamNode<TIn> : StreamNodeBase, ISortedStreamNodeOutput<TIn>
    {
        public ISortedStream<TIn> Output { get; }
        public TakeSortedStreamNode(ISortedStream<TIn> inputStream, string name, int count, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            this.Output = base.CreateSortedStream(nameof(Output), inputStream.Observable.Take(count), inputStream.SortCriterias);
        }
    }
    public class TakeKeyedStreamNode<TIn> : StreamNodeBase, IKeyedStreamNodeOutput<TIn>
    {
        public IKeyedStream<TIn> Output { get; }
        public TakeKeyedStreamNode(IKeyedStream<TIn> inputStream, string name, int count, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            this.Output = base.CreateKeyedStream(nameof(Output), inputStream.Observable.Take(count), inputStream.SortCriterias);
        }
    }
    public static partial class StreamEx
    {
        public static ISortedStream<TIn> Take<TIn>(this ISortedStream<TIn> stream, string name, int count)
        {
            return new TakeSortedStreamNode<TIn>(stream, name, count).Output;
        }
        public static IKeyedStream<TIn> Take<TIn>(this IKeyedStream<TIn> stream, string name, int count)
        {
            return new TakeKeyedStreamNode<TIn>(stream, name, count).Output;
        }
        public static IStream<TIn> Take<TIn>(this IStream<TIn> stream, string name, int count)
        {
            return new TakeStreamNode<TIn>(stream, name, count).Output;
        }
    }
}
