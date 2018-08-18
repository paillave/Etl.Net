using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System.Linq;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Disposables;
using System.Threading;
using Paillave.Etl.Core;
using System.Collections.ObjectModel;

namespace Paillave.Etl
{
    public class ExecutionContext<TConfig> : IConfigurable<TConfig>//, IDisposable
    {
        private readonly IPushSubject<TraceEvent> _traceSubject;
        private readonly IPushSubject<TConfig> _startupSubject;
        private readonly IExecutionContext _executionContext;
        private readonly IExecutionContext _traceExecutionContext;
        private readonly EventWaitHandle _startSynchronizer = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly List<StreamToNodeLink> _streamToNodeLinks = new List<StreamToNodeLink>();

        public IStream<TraceEvent> TraceStream { get; }
        public Stream<TConfig> StartupStream { get; }
        private TConfig _config;
        public IReadOnlyCollection<StreamToNodeLink> StreamToNodeLinks { get { return new ReadOnlyCollection<StreamToNodeLink>(_streamToNodeLinks); } }

        public ExecutionContext(string jobName)
        {
            this.JobName = jobName;

            this.ExecutionId = Guid.NewGuid();
            this._traceSubject = new PushSubject<TraceEvent>();
            this._startupSubject = new PushSubject<TConfig>();
            _traceExecutionContext = new TraceExecutionContext(this, _startSynchronizer);
            this.TraceStream = new Stream<TraceEvent>(null, _traceExecutionContext, null, null, this._traceSubject);
            _executionContext = new CurrentExecutionContext(this, _startSynchronizer, _streamToNodeLinks);
            this.StartupStream = new Stream<TConfig>(new Tracer(_executionContext, new CurrentExecutionNodeContext(jobName)), _executionContext, jobName, "Startup", this._startupSubject.First());
        }

        public void Configure(TConfig config)
        {
            this._config = config;
        }

        public Guid ExecutionId { get; }

        public Task ExecuteAsync(TConfig config)
        {
            Configure(config);
            return ExecuteAsync();
        }

        public Task ExecuteAsync()
        {
            _startSynchronizer.Set();
            this._startupSubject.PushValue(this._config);
            this._startupSubject.Complete();
            //System.Threading.Thread.Sleep(5000);
            //return Task.FromResult(0);
            return Task.WhenAll(
                this._executionContext.GetCompletionTask().ContinueWith(i =>
                {
                    _traceSubject.Complete();
                }),
                _traceExecutionContext.GetCompletionTask());
        }

        public string JobName { get; }

        private void Trace(TraceEvent traceEvent)
        {
            this._traceSubject.PushValue(traceEvent);
        }
        private class CurrentExecutionNodeContext : INodeContext
        {
            public CurrentExecutionNodeContext(string jobName)
            {
                this.NodeName = jobName;
            }
            public string NodeName { get; }

            public string TypeName => "ExecutionContext";
        }
        private class CurrentExecutionContext : IExecutionContext
        {
            private List<StreamToNodeLink> _streamToNodeLinks;
            private List<Task> _tasksToWait = new List<Task>();
            private CollectionDisposableManager _disposables = new CollectionDisposableManager();

            public CurrentExecutionContext(ExecutionContext<TConfig> localExecutionContext, WaitHandle startSynchronizer, List<StreamToNodeLink> streamToNodeLinks)
            {
                this._localExecutionContext = localExecutionContext;
                this.StartSynchronizer = startSynchronizer;
                this._streamToNodeLinks = streamToNodeLinks;
            }
            private ExecutionContext<TConfig> _localExecutionContext;
            public Guid ExecutionId => this._localExecutionContext.ExecutionId;
            public string JobName => this._localExecutionContext.JobName;
            public IPushObservable<TraceEvent> TraceEvents => this._localExecutionContext._traceSubject;

            public WaitHandle StartSynchronizer { get; }

            public void Trace(TraceEvent traceEvent) => this._localExecutionContext.Trace(traceEvent);

            public void AddToWaitForCompletion<T>(IPushObservable<T> stream)
            {
                _tasksToWait.Add(stream.ToTaskAsync());
            }

            public Task GetCompletionTask()
            {
                return Task.WhenAll(_tasksToWait.ToArray()).ContinueWith(_ => _disposables.Dispose());
            }

            public void AddDisposable(IDisposable disposable)
            {
                if (disposable != null)
                    _disposables.Set(disposable);
            }

            public void AddStreamToNodeLink(StreamToNodeLink link)
            {
                this._streamToNodeLinks.Add(link);
            }
        }
        private class TraceExecutionContext : IExecutionContext
        {
            private List<Task> _tasksToWait = new List<Task>();
            private ExecutionContext<TConfig> _localExecutionContext;
            private CollectionDisposableManager _disposables = new CollectionDisposableManager();
            public TraceExecutionContext(ExecutionContext<TConfig> localExecutionContext, WaitHandle startSynchronizer)
            {
                this._localExecutionContext = localExecutionContext;
                this.StartSynchronizer = startSynchronizer;
            }
            public Guid ExecutionId => this._localExecutionContext.ExecutionId;
            public string JobName => this._localExecutionContext.JobName;
            public IPushObservable<TraceEvent> TraceEvents => PushObservable.Empty<TraceEvent>();

            public WaitHandle StartSynchronizer { get; }

            public void Trace(TraceEvent traveEvent) { }

            public void AddToWaitForCompletion<T>(IPushObservable<T> stream)
            {
                _tasksToWait.Add(stream.ToTaskAsync());
            }

            public Task GetCompletionTask()
            {
                return Task.WhenAll(_tasksToWait.ToArray()).ContinueWith(_ =>
                {
                    _disposables.Dispose();
                });
            }

            public void AddDisposable(IDisposable disposable)
            {
                if (disposable != null)
                    _disposables.Set(disposable);
            }

            public void AddStreamToNodeLink(StreamToNodeLink link) { }
        }
    }
}
