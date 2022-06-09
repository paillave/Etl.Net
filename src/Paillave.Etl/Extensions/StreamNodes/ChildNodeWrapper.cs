namespace Paillave.Etl.Core
{
    public class ChildNodeWrapper : INodeContext
    {
        private INodeContext _parentNodeContext;
        public ChildNodeWrapper(INodeContext parentNodeContext)
            => (_parentNodeContext) = (parentNodeContext);
        public string NodeName => _parentNodeContext.NodeName;
        public string TypeName => _parentNodeContext.TypeName;
        public ProcessImpact PerformanceImpact => _parentNodeContext.PerformanceImpact;
        public ProcessImpact MemoryFootPrint => _parentNodeContext.MemoryFootPrint;
        // public ITraceEventFactory Tracer => _parentNodeContext.Tracer;
        public IExecutionContext ExecutionContext => _parentNodeContext.ExecutionContext;
        public INodeDescription Parent => _parentNodeContext;
        public TraceEvent CreateTraceEvent(ITraceContent content, int sequenceId)
            => new TraceEvent(this.ExecutionContext.JobName, this.ExecutionContext.ExecutionId, this.TypeName, this.NodeName, content, sequenceId);
    }
}
