namespace Paillave.Etl.Core
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
