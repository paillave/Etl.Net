using Paillave.Etl.Core.TraceContents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core
{
    public class Tracer : ITracer
    {
        private readonly IExecutionContext _executionContext;
        private readonly string _nodeName;
        private readonly string _typeName;

        public Tracer(IExecutionContext executionContext, INodeContext nodeContext, string nodeName = null)
        {
            this._executionContext = executionContext;
            this._nodeName = nodeName ?? nodeContext.NodeName;
            _typeName = nodeContext.TypeName;
        }

        public ITracer GetSubTracer(INodeContext nodeContext)
        {
            return new Tracer(_executionContext, nodeContext, $"{_nodeName}>{nodeContext.NodeName}");
        }

        public void Trace(ITraceContent content)
        {
            this._executionContext.Trace(new TraceEvent(_executionContext.JobName, _executionContext.ExecutionId, _typeName, _nodeName, content));
        }
    }
}
