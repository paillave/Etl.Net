using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.Extensions
{
    public static partial class SetForCorrelationEx
    {
        public static IStream<Correlated<TIn>> SetForCorrelation<TIn>(this IStream<TIn> stream, string name)
        {
            return new SetForCorrelationStreamNode<TIn>(name, new SetForCorrelationArgs<TIn>
            {
                Input = stream,
            }).Output;
        }
    }
}
