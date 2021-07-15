namespace Paillave.Etl.Core
{
    public class SubNodeWrapper : INodeContext
    {
        private INodeContext _parentNodeContext;

        public SubNodeWrapper(INodeContext parentNodeContext)
            => (_parentNodeContext, Tracer) = (parentNodeContext, parentNodeContext.Tracer.GetSubTraceMapper(parentNodeContext));

        public string NodeName => _parentNodeContext.NodeName;

        public string TypeName => _parentNodeContext.TypeName;

        public ProcessImpact PerformanceImpact => _parentNodeContext.PerformanceImpact;

        public ProcessImpact MemoryFootPrint => _parentNodeContext.MemoryFootPrint;

        public ITraceEventFactory Tracer { get; }

        public IExecutionContext ExecutionContext => _parentNodeContext.ExecutionContext;

        public bool IsRootNode => false;
    }
}
