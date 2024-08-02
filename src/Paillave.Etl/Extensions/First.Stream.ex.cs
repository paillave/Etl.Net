namespace Paillave.Etl.Core
{
    public static partial class FirstEx
    {
        public static ISingleStream<TIn> First<TIn>(this IStream<TIn> stream, string name) =>
            new FirstStreamNode<TIn>(name, new FirstArgs<TIn>
            {
                Input = stream,
            }).Output;
    }
}
