using System;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Reactive.Disposables;

namespace Paillave.Etl.Core.System
{
    public class ExecutionContext<TConfig> : IConfigurable<TConfig>//, IDisposable
    {
        private readonly ISubject<TraceEvent> _traceSubject;
        private readonly ISubject<TConfig> _startupSubject;
        public SortedStream<TraceEvent> TraceStream { get; }
        public Stream<TConfig> StartupStream { get; }
        private TConfig _config;
        private IList<IObservable<bool>> _streamsToEnd = new List<IObservable<bool>>();

        public ExecutionContext(string jobName)
        {
            this.JobName = jobName;
            this.ExecutionId = Guid.NewGuid();
            this._traceSubject = new Subject<TraceEvent>();
            this._startupSubject = new Subject<TConfig>();
            this.TraceStream = new SortedStream<TraceEvent>(null, new NullExecutionContext(this), null, this._traceSubject, new[] { new SortCriteria<TraceEvent>(i => i.DateTime) });
            var executionContext = new CurrentExecutionContext(this);
            this.StartupStream = new Stream<TConfig>(new Tracer(executionContext, new CurrentExecutionNodeContext(jobName)), executionContext, null, this._startupSubject.SingleAsync().Publish().RefCount());
            //this._streamToEnd = this.StartupStream.Observable.Select(i => true);
        }

        public void Configure(TConfig config)
        {
            this._config = config;
        }

        public Guid ExecutionId { get; }

        public async Task ExecuteAsync()
        {
            //this._streamsToEnd.Subscribe(_ => { }, () => this._traceSubject.OnCompleted());
            this._startupSubject.OnNext(this._config);
            this._startupSubject.OnCompleted();
            var output = Observable.Merge(this._streamsToEnd).TakeUntil(this._traceSubject.Where(i => i.Content.Level == TraceLevel.Error));
            output.Subscribe(_ => { }, this._traceSubject.OnCompleted);
            await output;
        }

        public string JobName { get; }

        private void Trace(TraceEvent traceEvent)
        {
            this._traceSubject.OnNext(traceEvent);
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
            public CurrentExecutionContext(ExecutionContext<TConfig> localExecutionContext)
            {
                this._localExecutionContext = localExecutionContext;
            }
            private ExecutionContext<TConfig> _localExecutionContext;
            public Guid ExecutionId => this._localExecutionContext.ExecutionId;
            public string JobName => this._localExecutionContext.JobName;
            public IObservable<TraceEvent> TraceEvents => this._localExecutionContext._traceSubject;
            public void Trace(TraceEvent traceEvent) => this._localExecutionContext.Trace(traceEvent);
            public void AddObservableToWait<TRow>(IObservable<TRow> observable) => _localExecutionContext._streamsToEnd.Add(observable.TakeUntil(_localExecutionContext._traceSubject.FirstOrDefaultAsync(i => i.Content.Level == TraceLevel.Error)).Select(i => true));
        }
        private class NullExecutionContext : IExecutionContext
        {
            private ExecutionContext<TConfig> _localExecutionContext;
            public NullExecutionContext(ExecutionContext<TConfig> localExecutionContext)
            {
                this._localExecutionContext = localExecutionContext;
            }
            public Guid ExecutionId { get; }
            public string JobName { get; }
            public IObservable<TraceEvent> TraceEvents => Observable.Empty<TraceEvent>();
            public void Trace(TraceEvent traveEvent) { }
            public void AddObservableToWait<TRow>(IObservable<TRow> observable) { }
        }
    }
}
