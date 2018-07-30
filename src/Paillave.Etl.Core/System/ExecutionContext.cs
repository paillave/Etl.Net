using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System.Linq;
using Paillave.Etl.Core.System.Streams;

namespace Paillave.Etl.Core.System
{
    public class ExecutionContext<TConfig> : IConfigurable<TConfig>//, IDisposable
    {
        private readonly IPushSubject<TraceEvent> _traceSubject;
        private readonly IPushSubject<TConfig> _startupSubject;
        private readonly IExecutionContext _executionContext;
        private readonly IExecutionContext _traceExecutionContext;
        //private readonly IPushObservable<TraceEvent> _forcedEndOfProcess;
        public IStream<TraceEvent> TraceStream { get; }
        //public SortedStream<TraceEvent> TraceStream { get; }
        public Stream<TConfig> StartupStream { get; }
        private TConfig _config;
        //private IList<IPushObservable<bool>> _streamsToEnd = new List<IPushObservable<bool>>();

        public ExecutionContext(string jobName)
        {
            this.JobName = jobName;
            this.ExecutionId = Guid.NewGuid();
            this._traceSubject = new PushSubject<TraceEvent>();
            //this._forcedEndOfProcess = this._traceSubject.Filter(i => i.Content.Level == TraceLevel.Error);
            this._startupSubject = new PushSubject<TConfig>();
            _traceExecutionContext = new NullExecutionContext(this);
            this.TraceStream = new Stream<TraceEvent>(null, _traceExecutionContext, null, this._traceSubject);
            //this.TraceStream = new SortedStream<TraceEvent>(null, _traceExecutionContext, null, this._traceSubject, new[] { new SortCriteria<TraceEvent>(i => i.DateTime) });
            _executionContext = new CurrentExecutionContext(this);
            this.StartupStream = new Stream<TConfig>(new Tracer(_executionContext, new CurrentExecutionNodeContext(jobName)), _executionContext, "Startup", this._startupSubject.First());
        }

        public void Configure(TConfig config)
        {
            this._config = config;
        }

        public Guid ExecutionId { get; }

        public Task ExecuteAsync()
        {
            this._startupSubject.PushValue(this._config);
            this._startupSubject.Complete();

            return Task.WhenAll(
                this._executionContext.GetCompletionTask().ContinueWith(i => _traceSubject.Complete()),
                _traceExecutionContext.GetCompletionTask());
        }

        public string JobName { get; }

        private void Trace(TraceEvent traceEvent)
        {
            this._traceSubject.PushValue(traceEvent);
        }
        private class CurrentExecutionNodeContext : INodeContext
        {
            private readonly string _jobName;

            public CurrentExecutionNodeContext(string jobName)
            {
                this._jobName = jobName;
            }
            public IEnumerable<string> NodeNamePath => new[] { _jobName };
            public string TypeName => "ExecutionContext";
        }
        private class CurrentExecutionContext : IExecutionContext
        {
            private List<Task> _tasksToWait = new List<Task>();

            public CurrentExecutionContext(ExecutionContext<TConfig> localExecutionContext)
            {
                this._localExecutionContext = localExecutionContext;
            }
            private ExecutionContext<TConfig> _localExecutionContext;
            public Guid ExecutionId => this._localExecutionContext.ExecutionId;
            public string JobName => this._localExecutionContext.JobName;
            public IPushObservable<TraceEvent> TraceEvents => this._localExecutionContext._traceSubject;
            public void Trace(TraceEvent traceEvent) => this._localExecutionContext.Trace(traceEvent);

            public void WaitCompletion<T>(IPushObservable<T> stream)
            {
                _tasksToWait.Add(stream.ToTaskAsync());
            }

            public Task GetCompletionTask()
            {
                return Task.WhenAll(_tasksToWait.ToArray());
            }
            //public IPushObservable<TRow> StopIfContextStops<TRow>(IPushObservable<TRow> observable)
            //{
            //    var ret = observable.TakeUntil(_localExecutionContext._forcedEndOfProcess);
            //    _localExecutionContext._streamsToEnd.Add(ret.Map(i => true));
            //    return ret;
            //}
        }
        private class NullExecutionContext : IExecutionContext
        {
            private List<Task> _tasksToWait = new List<Task>();
            private ExecutionContext<TConfig> _localExecutionContext;
            public NullExecutionContext(ExecutionContext<TConfig> localExecutionContext)
            {
                this._localExecutionContext = localExecutionContext;
            }
            public Guid ExecutionId => this._localExecutionContext.ExecutionId;
            public string JobName => this._localExecutionContext.JobName;
            public IPushObservable<TraceEvent> TraceEvents => PushObservable.Empty<TraceEvent>();

            public void Trace(TraceEvent traveEvent) { }

            public void WaitCompletion<T>(IPushObservable<T> stream)
            {
                _tasksToWait.Add(stream.ToTaskAsync());
            }

            public Task GetCompletionTask()
            {
                return Task.WhenAll(_tasksToWait.ToArray());
            }
            //public IPushObservable<TRow> StopIfContextStops<TRow>(IPushObservable<TRow> observable) => observable;
        }
    }
}
