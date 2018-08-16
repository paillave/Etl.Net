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
    public class EnsureKeyedStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, IEnumerable<SortCriteria<TIn>>>, IKeyedStreamNodeOutput<TIn>
    {
        public IKeyedStream<TIn> Output { get; }

        public EnsureKeyedStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, IEnumerable<SortCriteria<TIn>> arguments)
            : base(input, name, parentNodeNamePath, arguments)
        {
            this.Output = base.CreateKeyedStream(nameof(Output), input.Observable, arguments);
        }
    }
    public static partial class StreamEx
    {
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, params Expression<Func<TIn, IComparable>>[] sortFields)
        {
            return new EnsureKeyedStreamNode<TIn>(stream, name, null, sortFields.Select(i => new SortCriteria<TIn>(i)).ToList()).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, params SortCriteria<TIn>[] sortCriterias)
        {
            return new EnsureKeyedStreamNode<TIn>(stream, name, null, sortCriterias).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new EnsureKeyedStreamNode<TIn>(stream, name, null, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static IKeyedStream<TIn> EnsureKeyed<TIn>(this IStream<TIn> stream, string name, Func<TIn, SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new EnsureKeyedStreamNode<TIn>(stream, name, null, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
    }
}
