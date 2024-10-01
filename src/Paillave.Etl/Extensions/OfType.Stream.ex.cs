namespace Paillave.Etl.Core
{
    public static partial class OfTypeEx
    {
        public static IStream<TOut> OfType<TIn, TOut>(this IStream<TIn> stream, string name) where TOut : TIn =>
            new OfTypeStreamNode<TIn, TOut>(name, new OfTypeArgs<TIn, TOut>
            {
                Stream = stream,
            }).Output;
    }
}
