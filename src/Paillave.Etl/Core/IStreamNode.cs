using System;

namespace Paillave.Etl.Core
{
    public interface IStreamNode<TOut, TOutStream> : INodeDescription where TOutStream : IStream<TOut>
    {
        Guid IdNode { get; }
        IExecutionContext ExecutionContext { get; }
        TOutStream Output { get; }
        // public INodeDescription ParentNode { get; }
    }
}
