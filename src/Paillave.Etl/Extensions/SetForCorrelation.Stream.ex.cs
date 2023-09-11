namespace Paillave.Etl.Core
{
    public static partial class SetForCorrelationEx
    {
        public static IStream<Correlated<TIn>> SetForCorrelation<TIn>(this IStream<TIn> stream, string name)
            => new SetForCorrelationStreamNode<TIn>(name, new SetForCorrelationArgs<TIn> { Input = stream }).Output;

        public static IStream<TIn> Decorrelate<TIn>(this IStream<Correlated<TIn>> stream, string name)
            => new DecorrelateStreamNode<TIn>(name, new DecorrelateStreamNodeArgs<TIn> { Input = stream }).Output;
    }
}
