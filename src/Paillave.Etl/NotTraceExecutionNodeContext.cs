using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl
{
    internal class NotTraceExecutionNodeContext : INodeContext
    {
        public NotTraceExecutionNodeContext(IExecutionContext executionContext)
            => (ExecutionContext) = (executionContext);
        public string NodeName => string.Empty;
        public string TypeName => "ExecutionContext";
        public bool IsAwaitable => false;

        public ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public ITraceEventFactory Tracer => null;

        public IExecutionContext ExecutionContext { get; }

        public bool IsRootNode => true;
    }
}
