using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{
    internal class JobExecutionContext(string jobName, Guid executionId, 
        IPushSubject<TraceEvent> traceSubject, JobPoolDispatcher jobPoolDispatcher, 
        IDependencyResolver resolver, CancellationTokenSource internalCancellationTokenSource, 
        IFileValueConnectors connectors, bool useDetailedTraces) : IExecutionContext
    {
        private object _getOrCreateContextBagLock = new object();
        private JobPool _logJobPool = new JobPool();
        public TraceEvent EndOfProcessTraceEvent { get; private set; } = null;
        private readonly IPushSubject<TraceEvent> _traceSubject = traceSubject;
        private List<StreamToNodeLink> _streamToNodeLinks = new List<StreamToNodeLink>();
        private readonly JobPoolDispatcher _jobPoolDispatcher = jobPoolDispatcher;
        private readonly CancellationTokenSource _internalCancellationTokenSource = internalCancellationTokenSource;
        private List<INodeDescription> _nodes = new List<INodeDescription>();
        public JobDefinitionStructure GetDefinitionStructure() => new JobDefinitionStructure(_streamToNodeLinks, _nodes, this.JobName);
        private readonly List<Task> _tasksToWait = new List<Task>();
        private readonly CollectionDisposableManager _disposables = new CollectionDisposableManager();

        public IFileValueConnectors Connectors { get; } = connectors;
        public SimpleDependencyResolver ContextBag { get; } = new SimpleDependencyResolver();
        public IDependencyResolver DependencyResolver { get; } = resolver;
        public Guid ExecutionId { get; } = executionId;
        public string JobName { get; } = jobName;
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
        private object _traceSequenceLock = new object();
        public int NextTraceSequence()
        {
            lock (_traceSequenceLock)
                return ++_currentTraceSequence;
        }

        public Task InvokeInDedicatedThreadAsync(object threadOwner, Action action) => this._jobPoolDispatcher.InvokeAsync(threadOwner, action);
        public Task<T> InvokeInDedicatedThreadAsync<T>(object threadOwner, Func<T> action) => this._jobPoolDispatcher.InvokeAsync(threadOwner, action);
        public void AddTrace(ITraceContent traceContent, INodeContext sourceNode)
        {
            var traceEvent = sourceNode.CreateTraceEvent(traceContent, this.NextTraceSequence());
            _traceSubject?.PushValue(traceEvent);
            if (traceContent.Level == TraceLevel.Error && EndOfProcessTraceEvent == null)
            {
                EndOfProcessTraceEvent = traceEvent;
                Task.Run(_internalCancellationTokenSource.Cancel);
                // _traceSubject?.PushValue(traceEvent);
            }
        }

        public Task InvokeInDedicatedThreadAsync(object threadOwner, Func<Task> action) => this._jobPoolDispatcher.InvokeAsync(threadOwner, action);

        public Task<T> InvokeInDedicatedThreadAsync<T>(object threadOwner, Func<Task<T>> action) => this._jobPoolDispatcher.InvokeAsync(threadOwner, action);
    }
}
