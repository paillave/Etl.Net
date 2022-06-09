namespace Paillave.Etl.Core
{
    internal class CurrentExecutionNodeContext : INodeContext
    {
        public CurrentExecutionNodeContext(string jobName, IExecutionContext executionContext)
            => (NodeName, ExecutionContext) = (jobName, executionContext);
        public string NodeName { get; }
        public string TypeName => "ExecutionContext";
        public bool IsAwaitable => false;
        public ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public IExecutionContext ExecutionContext { get; }
        public INodeDescription Parent => null;
        public TraceEvent CreateTraceEvent(ITraceContent content, int sequenceId)
            => new TraceEvent(this.ExecutionContext.JobName, this.ExecutionContext.ExecutionId, this.TypeName, this.NodeName, content, sequenceId);
    }
}
