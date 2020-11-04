using Paillave.Etl.Core;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Connector;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    public class GetDefinitionExecutionContext : IExecutionContext
    {
        private List<StreamToNodeLink> _streamToNodeLinks = new List<StreamToNodeLink>();
        private List<INodeDescription> _nodes = new List<INodeDescription>();
        public JobDefinitionStructure GetDefinitionStructure() => new JobDefinitionStructure(_streamToNodeLinks, _nodes, this.JobName);
        private class RootNode : INodeDescription
        {
            public string NodeName => "-ProcessRunner-";
            public string TypeName => "-ProcessRunner-";
            public ProcessImpact PerformanceImpact => ProcessImpact.Light;
            public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
            public bool IsRootNode => true;
        }
        public GetDefinitionExecutionContext(string jobName, IFileValueConnectors connectors)
        {
            this.JobName = jobName;
            this.Connectors = connectors;
            _nodes.Add(new RootNode());
        }
        public void AddStreamToNodeLink(StreamToNodeLink link) => _streamToNodeLinks.Add(link);
        public Guid ExecutionId => Guid.Empty;
        public WaitHandle StartSynchronizer => throw new NotImplementedException();
        public string JobName { get; }
        public bool IsTracingContext => false;
        public void AddDisposable(IDisposable disposable) { }
        public void AddNode<T>(INodeDescription nodeContext, IPushObservable<T> observable) => _nodes.Add(nodeContext);
        public SimpleDependencyResolver ContextBag => new SimpleDependencyResolver();
        public IFileValueConnectors Connectors { get; }
        public IDependencyResolver DependencyResolver => new DummyDependencyResolver();
        public Task GetCompletionTask() => throw new NotImplementedException();
        public int NextTraceSequence() => 0;
        public void InvokeInDedicatedThread(object threadOwner, Action action) => throw new NotImplementedException();
        public T InvokeInDedicatedThread<T>(object threadOwner, Func<T> action) => throw new NotImplementedException();
        public object GetOrCreateFromContextBag(string key, Func<object> creator) => throw new NotImplementedException();
        public T GetOrCreateFromContextBag<T>(Func<T> creator) => throw new NotImplementedException();
        public void AddTrace(ITraceContent traceContent, INodeContext sourceNode) { }
    }
}
