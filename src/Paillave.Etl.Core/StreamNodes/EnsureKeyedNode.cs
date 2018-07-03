using Paillave.Etl.Core.System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class EnsureKeyedNode<TIn> : StreamNodeBase, IKeyedStreamNodeOutput<TIn>
    {
        public IKeyedStream<TIn> Output { get; }
        public EnsureKeyedNode(IStream<TIn> inputStream, string name, IEnumerable<SortCriteria<TIn>> sortCriterias, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            this.Output = base.CreateKeyedStream(nameof(Output), inputStream.Observable, sortCriterias);
        }
    }
    public static partial class StreamEx
    {
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, params SortCriteria<TIn>[] sortCriterias)
        {
            return new EnsureKeyedNode<TIn>(stream, name, sortCriterias).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new EnsureKeyedNode<TIn>(stream, name, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, Func<TIn, SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new EnsureKeyedNode<TIn>(stream, name, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
    }
}
