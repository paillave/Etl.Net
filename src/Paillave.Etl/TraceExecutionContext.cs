using Paillave.Etl.Core;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Connector;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl
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
        public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
        public void AddStreamToNodeLink(StreamToNodeLink link) { }
        public int NextTraceSequence() => 0;
        public void InvokeInDedicatedThread(object threadOwner, Action action) => this._jobPoolDispatcher.Invoke(threadOwner, action);
        public T InvokeInDedicatedThread<T>(object threadOwner, Func<T> action) => this._jobPoolDispatcher.Invoke(threadOwner, action);
        public object GetOrCreateFromContextBag(string key, Func<object> creator) => throw new NotImplementedException();
        public T GetOrCreateFromContextBag<T>(Func<T> creator) => throw new NotImplementedException();
        public void AddTrace(ITraceContent traceContent, INodeContext sourceNode) { }
    }
}
