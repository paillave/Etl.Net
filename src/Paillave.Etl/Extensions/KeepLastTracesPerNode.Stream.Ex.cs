using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.StreamNodes;

namespace Paillave.Etl.Extensions
{
    public static class KeepLastTracesPerNodeEx
    {
        public static IStream<NodeTraces> KeepLastTracesPerNode(this IStream<TraceEvent> traceEventStream, string name, int limit = 100)
            => new KeepLastTracesStreamNode(name, new KeepLastTracesArgs
            {
                InputStream = traceEventStream,
                Limit = limit
            }).Output;
    }
}
