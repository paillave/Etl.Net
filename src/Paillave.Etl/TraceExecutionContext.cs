using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    internal class TraceExecutionContext : IExecutionContext
    {
        private readonly IPushObservable<TraceEvent> _traceSubject;
        private readonly List<Task> _tasksToWait = new List<Task>();
        private readonly CollectionDisposableManager _disposables = new CollectionDisposableManager();
        private WaitHandle _startSynchronizer { get; }
        public TraceExecutionContext(WaitHandle startSynchronizer, Guid executionId)
        {
            this.ExecutionId = executionId;
            this.JobName = null;
            this._startSynchronizer = startSynchronizer;
            this._traceSubject = PushObservable.Empty<TraceEvent>(this._startSynchronizer);
        }
        public Guid ExecutionId { get; }
        public string JobName { get; }
        public bool IsTracingContext => true;
        public IPushObservable<TraceEvent> StopProcessEvent => PushObservable.Empty<TraceEvent>();
        //public void Trace(TraceEvent traveEvent) { }
        public void AddNode<T>(INodeContext nodeContext, IPushObservable<T> observable, IPushObservable<TraceEvent> traceObservable)
        {
            _tasksToWait.Add(observable.ToTaskAsync());
        }
        public Task GetCompletionTask() => Task.WhenAll(_tasksToWait.ToArray()).ContinueWith(_ => _disposables.Dispose());
        public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
        public void AddStreamToNodeLink(StreamToNodeLink link) { }
        public void Trace(TraceEvent traceEvent) { }
    }
}
