using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    public class GetDefinitionExecutionContext : IExecutionContext
    {
        private List<StreamToNodeLink> _streamToNodeLinks = new List<StreamToNodeLink>();
        private List<INodeContext> _nodes = new List<INodeContext>();
        public JobDefinitionStructure GetDefinitionStructure()
        {
            return new JobDefinitionStructure(_streamToNodeLinks, _nodes, this.JobName);
        }
        public GetDefinitionExecutionContext(INodeContext rootNode)
        {
            this.JobName = rootNode.NodeName;
            _nodes.Add(rootNode);
        }
        public void AddStreamToNodeLink(StreamToNodeLink link) => _streamToNodeLinks.Add(link);
        public Guid ExecutionId => throw new NotImplementedException();
        public WaitHandle StartSynchronizer => throw new NotImplementedException();
        public string JobName { get; }
        public bool IsTracingContext => false;
        public void AddDisposable(IDisposable disposable) => throw new NotImplementedException();
        public void AddNode<T>(INodeContext nodeContext, IPushObservable<T> observable, IPushObservable<TraceEvent> traceObservable) => _nodes.Add(nodeContext);
        public IPushObservable<TraceEvent> StopProcessEvent => PushObservable.Empty<TraceEvent>();
        public Task GetCompletionTask() => throw new NotImplementedException();
        public void Trace(TraceEvent traceEvent) { }
        //public void Trace(TraceEvent traceEvent) => throw new NotImplementedException();
    }
}
