using Paillave.Etl.Core.System.Streams;

namespace Paillave.Etl.Core.System.NodeOutputs
{
    public interface ISortedNodeOutputError<TOut, TIn>
    {
        IStream<ErrorRow<TIn>> Error { get; }
        ISortedStream<TOut> Output { get; }
    }
    public interface ISortedNodeOutputError<TOut, TIn1, TIn2>
    {
        IStream<ErrorRow<TIn1, TIn2>> Error { get; }
        ISortedStream<TOut> Output { get; }
    }
}