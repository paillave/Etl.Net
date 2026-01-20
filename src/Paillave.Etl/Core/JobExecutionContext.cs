using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Core;

internal class JobExecutionContext(Guid executionId, IPushSubject<TraceEvent> traceSubject, IServiceProvider services, CancellationTokenSource internalCancellationTokenSource, bool useDetailedTraces) : IExecutionContext
{
    public TraceEvent? EndOfProcessTraceEvent { get; private set; } = null;
    private readonly IPushSubject<TraceEvent> _traceSubject = traceSubject;
    private readonly List<StreamToNodeLink> _streamToNodeLinks = new();
    private readonly CancellationTokenSource _internalCancellationTokenSource = internalCancellationTokenSource;
    private readonly List<INodeDescription> _nodes = [];
    public JobDefinitionStructure GetDefinitionStructure() => new(_streamToNodeLinks, _nodes);
    private readonly List<Task> _tasksToWait = [];
    private readonly CollectionDisposableManager _disposables = new();

    public IServiceProvider Services { get; } = services;
    public Guid ExecutionId { get; } = executionId;
    public bool IsTracingContext => false;

    public bool Terminating => EndOfProcessTraceEvent != null;

    public bool UseDetailedTraces { get; } = useDetailedTraces;

    public void AddNode<T>(INodeDescription nodeContext, IPushObservable<T> observable)
    {
        _nodes.Add(nodeContext);
        _tasksToWait.Add(observable.ToEndAsync());
    }
    public void ReleaseResources() => _disposables.Dispose();
    public Task GetCompletionTask() => Task.WhenAll(_tasksToWait.ToArray());
    public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
    public void AddStreamToNodeLink(StreamToNodeLink link) => _streamToNodeLinks.Add(link);
    private int _currentTraceSequence = 0;
    private readonly object _traceSequenceLock = new();
    public int NextTraceSequence()
    {
        lock (_traceSequenceLock)
            return ++_currentTraceSequence;
    }

    public void AddTrace(ITraceContent traceContent, INodeContext sourceNode)
    {
        var traceEvent = sourceNode.CreateTraceEvent(traceContent, this.NextTraceSequence());
        _traceSubject?.PushValue(traceEvent);
        if (traceContent.Level == EtlTraceLevel.Error && EndOfProcessTraceEvent == null)
        {
            EndOfProcessTraceEvent = traceEvent;
            Task.Run(_internalCancellationTokenSource.Cancel);
        }
    }
}
