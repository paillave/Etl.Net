using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    internal class JobExecutionContext : IExecutionContext
    {
        private Func<IPushObservable<TraceEvent>, IPushObservable<TraceEvent>> _stopEventFilter;
        public TraceEvent EndOfProcessTraceEvent { get; private set; } = null;
        private readonly IPushSubject<TraceEvent> _traceSubject;
        private List<StreamToNodeLink> _streamToNodeLinks = new List<StreamToNodeLink>();
        private List<INodeContext> _nodes = new List<INodeContext>();
        public JobDefinitionStructure GetDefinitionStructure()
        {
            return new JobDefinitionStructure(_streamToNodeLinks, _nodes, this.JobName);
        }
        private readonly List<Task> _tasksToWait = new List<Task>();
        //private readonly List<Task> _tracesToWait = new List<Task>();
        private readonly CollectionDisposableManager _disposables = new CollectionDisposableManager();
        public IPushObservable<TraceEvent> StopProcessEvent { get; }
        public JobExecutionContext(string jobName, Guid executionId, IPushSubject<TraceEvent> traceSubject, Func<IPushObservable<TraceEvent>, IPushObservable<TraceEvent>> stopEventFilter)
        {
            this.ExecutionId = executionId;
            this.JobName = jobName;
            this._traceSubject = traceSubject;
            this.StopProcessEvent = stopEventFilter(traceSubject).Do(traceEvent => this.EndOfProcessTraceEvent = traceEvent);
        }
        public Guid ExecutionId { get; }
        public string JobName { get; }
        public bool IsTracingContext => false;
        //public void Trace(TraceEvent traceEvent) => _traceSubject.PushValue(traceEvent);
        public void AddNode<T>(INodeContext nodeContext, IPushObservable<T> observable, IPushObservable<TraceEvent> traceObservable)
        {
            _nodes.Add(nodeContext);
            _tasksToWait.Add(observable.ToTaskAsync());
            traceObservable.Do(i => this._traceSubject.PushValue(i));
            //_tracesToWait.Add(traceObservable.Do(this._traceObservable.PushValue).ToTaskAsync());
        }
        public void ReleaseResources()
        {
            _disposables.Dispose();
        }
        public Task GetCompletionTask()
        {
            var task = Task
                .WhenAll(_tasksToWait.ToArray());
            return task;
        }
        //public Task GetTraceCompletionTask()
        //{
        //    var task = Task
        //        .WhenAll(_tracesToWait.ToArray());
        //    return task;
        //}
        public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
        public void AddStreamToNodeLink(StreamToNodeLink link) => _streamToNodeLinks.Add(link);

        public void Trace(TraceEvent traceEvent)
        {
            throw new NotImplementedException();
        }
    }
}
