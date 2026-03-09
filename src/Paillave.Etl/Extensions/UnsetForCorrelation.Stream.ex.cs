namespace Paillave.Etl.Core;

public static partial class UnsetForCorrelationEx
{
    public static IStream<TIn> UnsetForCorrelation<TIn>(this IStream<Correlated<TIn>> stream, string name)
    {
        return new UnsetForCorrelationStreamNode<TIn>(name, new UnsetForCorrelationArgs<TIn>
        {
            Input = stream,
        }).Output;
    }
}
