using Paillave.Etl.Core.System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Paillave.Etl.Core.System.Streams;

namespace Paillave.Etl.Core.StreamNodes
{
    public class EnsureKeyedNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, IEnumerable<SortCriteria<TIn>>>, IKeyedStreamNodeOutput<TIn>
    {
        public IKeyedStream<TIn> Output { get; }

        public EnsureKeyedNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, IEnumerable<SortCriteria<TIn>> arguments)
            : base(input, name, parentNodeNamePath, arguments)
        {
            this.Output = base.CreateKeyedStream(nameof(Output), input.Observable, arguments);
        }
    }
    public static partial class StreamEx
    {
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, params Expression<Func<TIn, IComparable>>[] sortFields)
        {
            return new EnsureKeyedNode<TIn>(stream, name, null, sortFields.Select(i => new SortCriteria<TIn>(i)).ToList()).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, params SortCriteria<TIn>[] sortCriterias)
        {
            return new EnsureKeyedNode<TIn>(stream, name, null, sortCriterias).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new EnsureKeyedNode<TIn>(stream, name, null, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, Func<TIn, SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new EnsureKeyedNode<TIn>(stream, name, null, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
    }
}
