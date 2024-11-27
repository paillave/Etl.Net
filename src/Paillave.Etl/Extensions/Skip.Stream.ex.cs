namespace Paillave.Etl.Core
{
    public static partial class SkipEx
    {
        public static ISortedStream<TIn, TKey> Skip<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, int count) =>
            new SkipStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new SkipArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        public static IKeyedStream<TIn, TKey> Skip<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, int count) =>
            new SkipStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new SkipArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        public static IStream<TIn> Skip<TIn>(this IStream<TIn> stream, string name, int count) =>
            new SkipStreamNode<TIn, IStream<TIn>>(name, new SkipArgs<TIn, IStream<TIn>>
            {
                Input = stream,
                Count = count
            }).Output;
    }
}
