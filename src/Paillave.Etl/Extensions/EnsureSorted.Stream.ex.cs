using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SystemIO = System.IO;

namespace Paillave.Etl.Extensions
{
    public static partial class EnsureSortedEx
    {
        public static ISortedStream<TIn, TKey> EnsureSorted<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, object sortPositions = null)
        {
            return new EnsureSortedStreamNode<TIn, TKey>(name, new EnsureSortedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create(getKey, sortPositions)
            }).Output;
        }
        public static ISortedStream<TIn, TKey> EnsureSorted<TIn, TKey>(this IStream<TIn> stream, string name, SortDefinition<TIn, TKey> sortDefinition)
        {
            return new EnsureSortedStreamNode<TIn, TKey>(name, new EnsureSortedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = sortDefinition
            }).Output;
        }
        public static ISortedStream<Correlated<TIn>, TKey> EnsureSorted<TIn, TKey>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> getKey, object sortPositions = null)
        {
            return new EnsureSortedStreamNode<Correlated<TIn>, TKey>(name, new EnsureSortedArgs<Correlated<TIn>, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create((Correlated<TIn> i) => getKey(i.Row), sortPositions)
            }).Output;
        }
    }
}
