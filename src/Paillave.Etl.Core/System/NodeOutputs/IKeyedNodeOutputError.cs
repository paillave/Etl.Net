using Paillave.Etl.Core.System.Streams;

namespace Paillave.Etl.Core.System.NodeOutputs
{
    public interface IKeyedNodeOutputError<TOut, TIn>
    {
        IStream<ErrorRow<TIn>> Error { get; }
        IKeyedStream<TOut> Output { get; }
    }
    public interface IKeyedNodeOutputError<TOut, TIn1, TIn2>
    {
        IStream<ErrorRow<TIn1, TIn2>> Error { get; }
        IKeyedStream<TOut> Output { get; }
    }
}