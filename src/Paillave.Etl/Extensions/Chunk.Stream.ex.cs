using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public static partial class DistinctEx
    {
        public static IStream<IEnumerable<TIn>> Chunk<TIn>(this IStream<TIn> stream, string name, int chunkSize)
        {
            return new ChunkStreamNode<TIn>(name, new ChunkArgs<TIn>
            {
                ChunkSize = chunkSize,
                InputStream = stream
            }).Output;
        }
        public static IStream<Correlated<IEnumerable<TIn>>> Chunk<TIn>(this IStream<Correlated<TIn>> stream, string name, int chunkSize)
        {
            return new ChunkCorrelatedStreamNode<TIn>(name, new ChunkCorrelatedArgs<TIn>
            {
                ChunkSize = chunkSize,
                InputStream = stream
            }).Output;
        }
    }
}
