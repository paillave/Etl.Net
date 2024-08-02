using System;

namespace Paillave.Etl.Core
{
    public static partial class WhereEx
    {
        public static IKeyedStream<TIn, TKey> Where<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, Func<TIn, bool> predicate) =>
            new WhereStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new WhereArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Input = stream,
                Predicate = predicate
            }).Output;
        public static ISortedStream<TIn, TKey> Where<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, Func<TIn, bool> predicate) => new WhereStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new WhereArgs<TIn, ISortedStream<TIn, TKey>>
        {
            Input = stream,
            Predicate = predicate
        }).Output;
        public static IStream<TIn> Where<TIn>(this IStream<TIn> stream, string name, Func<TIn, bool> predicate) =>
            new WhereStreamNode<TIn, IStream<TIn>>(name, new WhereArgs<TIn, IStream<TIn>>
            {
                Input = stream,
                Predicate = predicate
            }).Output;
        public static IStream<Correlated<TIn>> WhereCorrelated<TIn>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, bool> predicate) =>
            new WhereStreamNode<Correlated<TIn>, IStream<Correlated<TIn>>>(name, new WhereArgs<Correlated<TIn>, IStream<Correlated<TIn>>>
            {
                Input = stream,
                Predicate = i => predicate(i.Row)
            }).Output;
        public static IKeyedStream<Correlated<TIn>, TKey> Where<TIn, TKey>(this IKeyedStream<Correlated<TIn>, TKey> stream, string name, Func<TIn, bool> predicate) =>
            new WhereStreamNode<Correlated<TIn>, IKeyedStream<Correlated<TIn>, TKey>>(name, new WhereArgs<Correlated<TIn>, IKeyedStream<Correlated<TIn>, TKey>>
            {
                Input = stream,
                Predicate = i => predicate(i.Row)
            }).Output;
        public static ISortedStream<Correlated<TIn>, TKey> Where<TIn, TKey>(this ISortedStream<Correlated<TIn>, TKey> stream, string name, Func<TIn, bool> predicate) =>
            new WhereStreamNode<Correlated<TIn>, ISortedStream<Correlated<TIn>, TKey>>(name, new WhereArgs<Correlated<TIn>, ISortedStream<Correlated<TIn>, TKey>>
            {
                Input = stream,
                Predicate = i => predicate(i.Row)
            }).Output;
        public static IStream<Correlated<TIn>> Where<TIn>(this IStream<Correlated<TIn>> stream, string name, Func<TIn, bool> predicate) =>
            new WhereStreamNode<Correlated<TIn>, IStream<Correlated<TIn>>>(name, new WhereArgs<Correlated<TIn>, IStream<Correlated<TIn>>>
            {
                Input = stream,
                Predicate = i => predicate(i.Row)
            }).Output;
    }
}
