namespace Paillave.Etl.Core
{
    public static partial class TopEx
    {
        public static ISortedStream<TIn, TKey> Top<TIn, TKey>(this ISortedStream<TIn, TKey> stream, string name, int count)
        {
            return new TopStreamNode<TIn, ISortedStream<TIn, TKey>>(name, new TopArgs<TIn, ISortedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        public static IKeyedStream<TIn, TKey> Top<TIn, TKey>(this IKeyedStream<TIn, TKey> stream, string name, int count)
        {
            return new TopStreamNode<TIn, IKeyedStream<TIn, TKey>>(name, new TopArgs<TIn, IKeyedStream<TIn, TKey>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
        public static IStream<TIn> Top<TIn>(this IStream<TIn> stream, string name, int count)
        {
            return new TopStreamNode<TIn, IStream<TIn>>(name, new TopArgs<TIn, IStream<TIn>>
            {
                Input = stream,
                Count = count
            }).Output;
        }
    }
}
