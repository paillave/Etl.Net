using System;

namespace Paillave.Etl.Core
{
    public interface IStreamNode<TOut, TOutStream> where TOutStream : IStream<TOut>
    {
        Guid IdNode { get; }
        IExecutionContext ExecutionContext { get; }
        ITraceEventFactory Tracer { get; }
        ProcessImpact PerformanceImpact { get; }
        ProcessImpact MemoryFootPrint { get; }
        string NodeName { get; }
        string TypeName { get; }
        TOutStream Output { get; }
        bool IsRootNode { get; }
    }
}
