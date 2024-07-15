namespace Paillave.Etl.Core
{
    public static partial class LastEx
    {
        public static ISingleStream<TIn> Last<TIn>(this IStream<TIn> stream, string name) =>
            new LastStreamNode<TIn>(name, new LastArgs<TIn>
            {
                Input = stream,
            }).Output;
    }
}
