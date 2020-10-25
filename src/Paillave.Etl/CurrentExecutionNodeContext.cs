using Paillave.Etl.Core;

namespace Paillave.Etl
{
    internal class CurrentExecutionNodeContext : INodeContext
    {
        public CurrentExecutionNodeContext(string jobName, ITraceEventFactory tracer, IExecutionContext executionContext)
            => (NodeName, Tracer, ExecutionContext) = (jobName, tracer, executionContext);
        public string NodeName { get; }
        public string TypeName => "ExecutionContext";
        public bool IsAwaitable => false;
        public ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public ITraceEventFactory Tracer { get; }
        public IExecutionContext ExecutionContext { get; }
        public bool IsRootNode => true;
    }
}
