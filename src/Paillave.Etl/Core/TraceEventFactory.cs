using Paillave.Etl.Core;
using System;

namespace Paillave.Etl.Core
{
    public class TraceEventFactory : ITraceEventFactory
    {
        private readonly string _nodeName;
        private readonly string _typeName;
        private readonly Guid _executionId;
        private readonly string _jobName;
        private readonly INodeDescription _nodeContext = null;
        public TraceEventFactory(IExecutionContext executionContext, INodeDescription nodeContext)
        {
            this._jobName = executionContext.JobName;
            this._executionId = executionContext.ExecutionId;
            this._nodeName = nodeContext.NodeName;
            this._nodeContext = nodeContext;
        }
        public TraceEventFactory(IExecutionContext executionContext)
        {
            this._jobName = executionContext.JobName;
            this._executionId = executionContext.ExecutionId;
            this._nodeName = "-ProcessRunner-";
            _typeName = "-ProcessRunner-";
        }
        private TraceEventFactory(string nodeName, string typeName, Guid executionId, string jobName)
        {
            _nodeName = nodeName;
            _typeName = typeName;
            _executionId = executionId;
            _jobName = jobName;
        }
        public ITraceEventFactory GetSubTraceMapper(INodeContext nodeContext)
            => new TraceEventFactory($"{_nodeName}>{nodeContext.NodeName}", nodeContext.TypeName, this._executionId, this._jobName);
        public TraceEvent CreateTraceEvent(ITraceContent content, int sequenceId)
            => new TraceEvent(_jobName, _executionId, this._nodeContext?.TypeName ?? _typeName, _nodeName, content, sequenceId);
    }
}
