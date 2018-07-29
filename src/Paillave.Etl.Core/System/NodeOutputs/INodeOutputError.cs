using Paillave.Etl.Core.System.Streams;

namespace Paillave.Etl.Core.System.NodeOutputs
{
    public interface INodeOutputError<TOut, TIn>
    {
        IStream<ErrorRow<TIn>> Error { get; }
        IStream<TOut> Output { get; }
    }
    public interface INodeOutputError<TOut, TIn1, TIn2>
    {
        IStream<ErrorRow<TIn1, TIn2>> Error { get; }
        IStream<TOut> Output { get; }
    }
}