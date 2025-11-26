namespace Paillave.Etl.Core
{
    internal class NotTraceExecutionNodeContext(IExecutionContext executionContext) : INodeContext
    {
        public string NodeName => string.Empty;
        public string TypeName => "ExecutionContext";
        public bool IsAwaitable => false;
        public ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public IExecutionContext ExecutionContext { get; } = executionContext;
        public INodeDescription? Parent => null;
        public TraceEvent CreateTraceEvent(ITraceContent content, int sequenceId)
            => new TraceEvent(this.ExecutionContext.ExecutionId, this.TypeName, this.NodeName, content, sequenceId);
    }
}
