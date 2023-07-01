using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public interface INodeContext : INodeDescription
    {
        TraceEvent CreateTraceEvent(ITraceContent content, int sequenceId);
        IExecutionContext ExecutionContext { get; }
    }
}
