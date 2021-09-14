using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public interface INodeContext : INodeDescription
    {
        ITraceEventFactory Tracer { get; }
        IExecutionContext ExecutionContext { get; }
    }
}
