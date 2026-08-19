using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Core;

public class GetDefinitionExecutionContext(IServiceProvider? services = null) : IExecutionContext
{
    private readonly List<StreamToNodeLink> _streamToNodeLinks = [];
    private readonly List<INodeDescription> _nodes = [];
    public JobDefinitionStructure GetDefinitionStructure() => new(_streamToNodeLinks, _nodes);
    public void AddStreamToNodeLink(StreamToNodeLink link) => _streamToNodeLinks.Add(link);
    public Guid ExecutionId => Guid.Empty;
    public WaitHandle StartSynchronizer => throw new NotImplementedException();
    public bool IsTracingContext => false;
    public void AddDisposable(IDisposable? disposable) { }
    public void AddUnderlyingDisposables(StreamWithResource disposable) { }
    public void AddNode<T>(INodeDescription nodeContext, IPushObservable<T> observable) => _nodes.Add(nodeContext);
    // public IMemoryCache ContextBag => new MemoryCache();
    // public IFileValueConnectors Connectors { get; }
    // Nodes that need a DI service at graph-construction time (e.g. FromConnector resolving
    // IFileValueConnectors) can't get it from anywhere else, since GetDefinitionStructure() never
    // runs the pipeline — the caller is the only one who knows what's safe to stand in here.
    public IServiceProvider Services { get; } = services ?? new ServiceCollection().BuildServiceProvider();

    public bool Terminating => throw new NotImplementedException();

    public bool UseDetailedTraces => false;

    public Task GetCompletionTask() => throw new NotImplementedException();
    public int NextTraceSequence() => 0;
    public void AddTrace(ITraceContent traceContent, INodeContext sourceNode) { }
}
