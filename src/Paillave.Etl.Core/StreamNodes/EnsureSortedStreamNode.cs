using Paillave.Etl.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{
    public class EnsureSortedStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, IEnumerable<SortCriteria<TIn>>>, ISortedStreamNodeOutput<TIn>
    {
        public ISortedStream<TIn> Output { get; }

        public EnsureSortedStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, IEnumerable<SortCriteria<TIn>> arguments) 
            : base(input, name, parentNodeNamePath, arguments)
        {
            this.Output = base.CreateSortedStream(nameof(Output), input.Observable, arguments);
        }
    }
    public static partial class StreamEx
    {
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, params Expression<Func<TIn, IComparable>>[] sortFields)
        {
            return new EnsureSortedStreamNode<TIn>(stream, name, null, sortFields.Select(i => new SortCriteria<TIn>(i)).ToList()).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, params SortCriteria<TIn>[] sortCriterias)
        {
            return new EnsureSortedStreamNode<TIn>(stream, name, null,sortCriterias).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new EnsureSortedStreamNode<TIn>(stream, name, null, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static ISortedStream<TIn> EnsureSorted<TIn>(this IStream<TIn> stream, string name, Func<TIn, SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new EnsureSortedStreamNode<TIn>(stream, name, null, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
    }
}
