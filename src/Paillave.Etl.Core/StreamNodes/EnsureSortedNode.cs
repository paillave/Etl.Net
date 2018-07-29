using Paillave.Etl.Core.System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Paillave.Etl.Core.System.Streams;

namespace Paillave.Etl.Core.StreamNodes
{
    public class EnsureSortedNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, IEnumerable<SortCriteria<TIn>>>, ISortedStreamNodeOutput<TIn>
    {
        public ISortedStream<TIn> Output { get; }

        public EnsureSortedNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, IEnumerable<SortCriteria<TIn>> arguments) 
            : base(input, name, parentNodeNamePath, arguments)
        {
            this.Output = base.CreateSortedStream(nameof(Output), input.Observable, arguments);
        }
    }
    public static partial class StreamEx
    {
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, params Expression<Func<TIn, IComparable>>[] sortFields)
        {
            return new EnsureSortedNode<TIn>(stream, name, null, sortFields.Select(i => new SortCriteria<TIn>(i)).ToList()).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, params SortCriteria<TIn>[] sortCriterias)
        {
            return new EnsureSortedNode<TIn>(stream, name, null,sortCriterias).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new EnsureSortedNode<TIn>(stream, name, null, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, Func<TIn, SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new EnsureSortedNode<TIn>(stream, name, null, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
    }
}
