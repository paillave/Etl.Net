using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class MergeStreamNode<TIn> : StreamNodeBase, IStreamNodeOutput<TIn>
    {
        public IStream<TIn> Output { get; }
        public MergeStreamNode(IStream<TIn> inputStream1, IStream<TIn> inputStream2, string name, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream1.ExecutionContext ?? inputStream2.ExecutionContext, name, parentNodeNamePath);
            this.Output = base.CreateStream(nameof(Output), inputStream1.Observable.Merge(inputStream2.Observable));
        }
    }
    public static partial class StreamEx
    {
        public static IStream<I> Merge<I>(this IStream<I> stream, string name, IStream<I> inputStream2)
        {
            return new MergeStreamNode<I>(stream, inputStream2, name).Output;
        }
    }
}
