using Paillave.Etl.Reactive.Core;
using System;

namespace Paillave.Etl.Core
{
    public static partial class EnsureSortedEx
    {
        public static ISortedStream<TIn, TKey> EnsureSorted<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, object sortPositions = null) =>
            new EnsureSortedStreamNode<TIn, TKey>(name, new EnsureSortedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create(getKey, sortPositions)
            }).Output;
        public static ISortedStream<TIn, TKey> EnsureSorted<TIn, TKey>(this IStream<TIn> stream, string name, SortDefinition<TIn, TKey> sortDefinition) =>
            new EnsureSortedStreamNode<TIn, TKey>(name, new EnsureSortedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = sortDefinition
            }).Output;
        public static ISortedStream<Correlated<TIn>, TKey> EnsureSorted<TIn, TKey>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> getKey, object sortPositions = null) =>
            new EnsureSortedStreamNode<Correlated<TIn>, TKey>(name, new EnsureSortedArgs<Correlated<TIn>, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create((Correlated<TIn> i) => getKey(i.Row), sortPositions)
            }).Output;
    }
}
