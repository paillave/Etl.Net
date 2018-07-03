using Paillave.Etl.Core.System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class EnsureSortedNode<TIn> : StreamNodeBase, ISortedStreamNodeOutput<TIn>
    {
        public ISortedStream<TIn> Output { get; }
        public EnsureSortedNode(IStream<TIn> inputStream, string name, IEnumerable<SortCriteria<TIn>> sortCriterias, IEnumerable<string> parentNodeNamePath = null)
        {
            base.Initialize(inputStream.ExecutionContext, name, parentNodeNamePath);
            this.Output = base.CreateSortedStream(nameof(Output), inputStream.Observable, sortCriterias);
        }
    }
    public static partial class StreamEx
    {
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, params SortCriteria<TIn>[] sortCriterias)
        {
            return new EnsureSortedNode<TIn>(stream, name, sortCriterias).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new EnsureSortedNode<TIn>(stream, name, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, Func<TIn, SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new EnsureSortedNode<TIn>(stream, name, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
    }
}
