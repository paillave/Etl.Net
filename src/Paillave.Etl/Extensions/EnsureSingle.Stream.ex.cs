namespace Paillave.Etl.Core
{
    public static partial class EnsureSingleEx
    {
        public static ISingleStream<TIn> EnsureSingle<TIn>(this IStream<TIn> stream, string name)
        {
            return new EnsureSingleStreamNode<TIn>(name, new EnsureSingleArgs<TIn>
            {
                Input = stream
            }).Output;
        }
    }
}
