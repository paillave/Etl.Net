using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core
{
    public class TraceMapper : ITraceMapper
    {
        private readonly IExecutionContext _executionContext;
        private readonly string _nodeName;
        private readonly string _typeName;

        public TraceMapper(IExecutionContext executionContext, INodeContext nodeContext, string nodeName = null)
        {
            this._executionContext = executionContext;
            this._nodeName = nodeName ?? nodeContext.NodeName;
            _typeName = nodeContext.TypeName;
        }

        public ITraceMapper GetSubTraceMapper(INodeContext nodeContext)
        {
            return new TraceMapper(_executionContext, nodeContext, $"{_nodeName}>{nodeContext.NodeName}");
        }

        public TraceEvent MapToTrace(ITraceContent content)
        {
            return new TraceEvent(_executionContext.JobName, _executionContext.ExecutionId, _typeName, _nodeName, content);
        }
    }
}
