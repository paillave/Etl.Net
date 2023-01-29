using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{
    internal class TraceExecutionContext : IExecutionContext
    {
        private readonly List<Task> _tasksToWait = new List<Task>();
        private readonly CollectionDisposableManager _disposables = new CollectionDisposableManager();
        private WaitHandle _startSynchronizer { get; }
        public TraceExecutionContext(WaitHandle startSynchronizer, Guid executionId, JobPoolDispatcher jobPoolDispatcher, IDependencyResolver resolver, CancellationToken cancellationToken, IFileValueConnectors connectors)
        {
            this._jobPoolDispatcher = jobPoolDispatcher;
            this.ExecutionId = executionId;
            this.Connectors = connectors;
            this.JobName = null;
            this._startSynchronizer = startSynchronizer;
            this.DependencyResolver = resolver;
            this.ContextBag = new SimpleDependencyResolver();
        }
        public IDependencyResolver DependencyResolver { get; }
        private readonly JobPoolDispatcher _jobPoolDispatcher;
        public IFileValueConnectors Connectors { get; }
        public Guid ExecutionId { get; }
        public string JobName { get; }
        public bool IsTracingContext => true;
        public void AddNode<T>(INodeDescription nodeContext, IPushObservable<T> observable) => _tasksToWait.Add(observable.ToTaskAsync());
        public Task GetCompletionTask() => Task.WhenAll(_tasksToWait.ToArray()).ContinueWith(_ => _disposables.Dispose());
        public SimpleDependencyResolver ContextBag { get; }

        public bool Terminating => false;

        public bool UseDetailedTraces => false;

        public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
        public void AddStreamToNodeLink(StreamToNodeLink link) { }
        public int NextTraceSequence() => 0;
        public Task InvokeInDedicatedThreadAsync(object threadOwner, Action action) => this._jobPoolDispatcher.InvokeAsync(threadOwner, action);
        public Task<T> InvokeInDedicatedThreadAsync<T>(object threadOwner, Func<T> action) => this._jobPoolDispatcher.InvokeAsync(threadOwner, action);
        public object GetOrCreateFromContextBag(string key, Func<object> creator) => throw new NotImplementedException();
        public T GetOrCreateFromContextBag<T>(Func<T> creator) => throw new NotImplementedException();
        public void AddTrace(ITraceContent traceContent, INodeContext sourceNode) { }
        public void AddUnderlyingDisposables(StreamWithResource disposable) => _disposables.AddRange(disposable.UnderlyingDisposables.ToArray());
    }
}
