using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{
    internal class TraceExecutionContext(WaitHandle startSynchronizer, Guid executionId, IServiceProvider services, CancellationToken cancellationToken) : IExecutionContext
    {
        private readonly List<Task> _tasksToWait = new List<Task>();
        private readonly CollectionDisposableManager _disposables = new CollectionDisposableManager();
        private WaitHandle _startSynchronizer { get; } = startSynchronizer;
        public IServiceProvider Services { get; } = services;
        public Guid ExecutionId { get; } = executionId;
        public bool IsTracingContext => true;
        public void AddNode<T>(INodeDescription nodeContext, IPushObservable<T> observable) => _tasksToWait.Add(observable.ToTaskAsync());
        public Task GetCompletionTask() => Task.WhenAll(_tasksToWait.ToArray()).ContinueWith(_ => _disposables.Dispose());

        public bool Terminating => false;

        public bool UseDetailedTraces => false;

        public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
        public void AddStreamToNodeLink(StreamToNodeLink link) { }
        public int NextTraceSequence() => 0;
        public void AddTrace(ITraceContent traceContent, INodeContext sourceNode) { }
    }
}
