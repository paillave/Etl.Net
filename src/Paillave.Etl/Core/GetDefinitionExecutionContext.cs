using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{
    public class GetDefinitionExecutionContext : IExecutionContext
    {
        private readonly List<StreamToNodeLink> _streamToNodeLinks = [];
        private readonly List<INodeDescription> _nodes = [];
        public JobDefinitionStructure GetDefinitionStructure() => new JobDefinitionStructure(_streamToNodeLinks, _nodes);
        public void AddStreamToNodeLink(StreamToNodeLink link) => _streamToNodeLinks.Add(link);
        public Guid ExecutionId => Guid.Empty;
        public WaitHandle StartSynchronizer => throw new NotImplementedException();
        public bool IsTracingContext => false;
        public void AddDisposable(IDisposable? disposable) { }
        public void AddUnderlyingDisposables(StreamWithResource disposable) { }
        public void AddNode<T>(INodeDescription nodeContext, IPushObservable<T> observable) => _nodes.Add(nodeContext);
        // public IMemoryCache ContextBag => new MemoryCache();
        // public IFileValueConnectors Connectors { get; }
        public IServiceProvider Services { get; } = new ServiceCollection().BuildServiceProvider();

        public bool Terminating => throw new NotImplementedException();

        public bool UseDetailedTraces => false;

        public Task GetCompletionTask() => throw new NotImplementedException();
        public int NextTraceSequence() => 0;
        public void AddTrace(ITraceContent traceContent, INodeContext sourceNode) { }
    }
}
