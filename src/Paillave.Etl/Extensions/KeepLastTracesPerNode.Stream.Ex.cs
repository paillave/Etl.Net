namespace Paillave.Etl.Core;

public static class KeepLastTracesPerNodeEx
{
    public static IStream<NodeTraces> KeepLastTracesPerNode(this IStream<TraceEvent> traceEventStream, string name, int limit = 100)
        => new KeepLastTracesStreamNode(name, new KeepLastTracesArgs
        {
            InputStream = traceEventStream,
            Limit = limit
        }).Output;
}
