namespace Paillave.Etl.Core;

internal class CurrentExecutionNodeContext(string jobName, IExecutionContext executionContext) : INodeContext
{
    public string NodeName => jobName;
    public string TypeName => "ExecutionContext";
    public bool IsAwaitable => false;
    public ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    public IExecutionContext ExecutionContext => executionContext;
    public INodeDescription? Parent => null;
    public TraceEvent CreateTraceEvent(ITraceContent content, int sequenceId)
        => new(this.ExecutionContext.ExecutionId, this.TypeName, this.NodeName, content, sequenceId);
}
