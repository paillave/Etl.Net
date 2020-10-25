using Paillave.Etl.StreamNodes;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.Extensions
{
    public static partial class LastEx
    {
        public static ISingleStream<TIn> Last<TIn>(this IStream<TIn> stream, string name)
        {
            return new LastStreamNode<TIn>(name, new LastArgs<TIn>
            {
                Input = stream,
            }).Output;
        }
    }
}
