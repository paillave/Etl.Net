using Paillave.Etl.Reactive.Core;
using System;

namespace Paillave.Etl.Core
{
    public static partial class EnsureKeyedEx
    {
        public static IKeyedStream<TIn, TKey> EnsureKeyed<TIn, TKey>(this IStream<TIn> stream, string name, Func<TIn, TKey> getKey, object sortPositions = null) =>
            new EnsureKeyedStreamNode<TIn, TKey>(name, new EnsureKeyedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create(getKey, sortPositions)
            }).Output;
        public static IKeyedStream<TIn, TKey> EnsureKeyed<TIn, TKey>(this IStream<TIn> stream, string name, SortDefinition<TIn, TKey> sortDefinition) =>
            new EnsureKeyedStreamNode<TIn, TKey>(name, new EnsureKeyedArgs<TIn, TKey>
            {
                Input = stream,
                SortDefinition = sortDefinition
            }).Output;
        public static IKeyedStream<Correlated<TIn>, TKey> EnsureKeyed<TIn, TKey>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, TKey> getKey, object sortPositions = null) =>
            new EnsureKeyedStreamNode<Correlated<TIn>, TKey>(name, new EnsureKeyedArgs<Correlated<TIn>, TKey>
            {
                Input = stream,
                SortDefinition = SortDefinition.Create((Correlated<TIn> i) => getKey(i.Row), sortPositions)
            }).Output;
    }
}
