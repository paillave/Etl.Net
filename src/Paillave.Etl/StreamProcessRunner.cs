using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    public class StreamProcessRunner
    {
        public static StreamProcessRunner<TConfig> Create<TConfig>(Action<ISingleStream<TConfig>> jobDefinition, string jobName = "NoName") => new StreamProcessRunner<TConfig>(jobDefinition, jobName);
        public static Task<ExecutionStatus> CreateAndExecuteAsync<TConfig>(TConfig config, Action<ISingleStream<TConfig>> jobDefinition, Action<IStream<TraceEvent>> traceProcessDefinition = null, string jobName = "NoName") => new StreamProcessRunner<TConfig>(jobDefinition, jobName).ExecuteAsync(config, traceProcessDefinition);
        public static Task<ExecutionStatus> CreateAndExecuteWithNoFaultAsync<TConfig>(TConfig config, Action<ISingleStream<TConfig>> jobDefinition, Action<IStream<TraceEvent>> traceProcessDefinition = null, string jobName = "NoName") => new StreamProcessRunner<TConfig>(jobDefinition, jobName).ExecuteWithNoFaultAsync(config, traceProcessDefinition);
    }

    public class StreamProcessRunner<TConfig> : IStreamProcessRunner
    {
        private Action<ISingleStream<TConfig>> _jobDefinition;
        private string _jobName;
        private Func<IPushObservable<TraceEvent>, IPushObservable<TraceEvent>> _defaultStopCondition = traces => traces.Filter(i => i.Content.Level == TraceLevel.Error).First();
        public Func<IPushObservable<TraceEvent>, IPushObservable<TraceEvent>> StopCondition { get; set; }
        public StreamProcessRunner(Action<ISingleStream<TConfig>> jobDefinition, string jobName = "NoName")
        {
            _jobDefinition = jobDefinition ?? (_jobDefinition => { });
            _jobName = jobName;
        }
        Task<ExecutionStatus> IStreamProcessRunner.ExecuteWithNoFaultAsync(object config, Action<IStream<TraceEvent>> traceProcessDefinition = null)
        {
            return ExecuteWithNoFaultAsync((TConfig)config, traceProcessDefinition);
        }
        public Task<ExecutionStatus> ExecuteWithNoFaultAsync(TConfig config, Action<IStream<TraceEvent>> traceProcessDefinition = null)
        {
            return ExecuteAsync(config, traceProcessDefinition, true);
        }
        public Task<ExecutionStatus> ExecuteAsync(TConfig config, Action<IStream<TraceEvent>> traceProcessDefinition = null, bool noExceptionOnError = false)
        {
            Guid executionId = Guid.NewGuid();
            EventWaitHandle startSynchronizer = new EventWaitHandle(false, EventResetMode.ManualReset);
            IPushSubject<TraceEvent> traceSubject = new PushSubject<TraceEvent>();
            IPushSubject<TConfig> startupSubject = new PushSubject<TConfig>();
            IExecutionContext traceExecutionContext = new TraceExecutionContext(startSynchronizer, executionId);
            var traceStream = new Stream<TraceEvent>(null, traceExecutionContext, null, traceSubject);
            JobExecutionContext jobExecutionContext = new JobExecutionContext(_jobName, executionId, traceSubject, this.StopCondition ?? _defaultStopCondition);
            var startupStream = new SingleStream<TConfig>(new Tracer(jobExecutionContext, new CurrentExecutionNodeContext(_jobName)), jobExecutionContext, _jobName, startupSubject.First());
            if (traceProcessDefinition != null) traceProcessDefinition(traceStream);
            _jobDefinition(startupStream);
            Task<StreamStatistics> jobExecutionStatusTask = traceStream.GetStreamStatisticsAsync();
            var task = Task.WhenAll(
                jobExecutionContext
                    .GetCompletionTask()
                    .ContinueWith(_ =>
                    {
                        traceSubject.Complete();
                    }),
                traceExecutionContext
                    .GetCompletionTask()
                )
                .ContinueWith(t =>
                {
                    jobExecutionContext.ReleaseResources();
                    if (jobExecutionContext.EndOfProcessTraceEvent != null && !noExceptionOnError)
                        throw new JobExecutionException(jobExecutionContext.EndOfProcessTraceEvent);
                    jobExecutionStatusTask.Wait();
                    return new ExecutionStatus(jobExecutionContext.GetDefinitionStructure(), jobExecutionStatusTask.Result, jobExecutionContext.EndOfProcessTraceEvent);
                });
            startSynchronizer.Set();
            startupSubject.PushValue(config);
            startupSubject.Complete();
            return task;
        }
        public JobDefinitionStructure GetDefinitionStructure()
        {
            GetDefinitionExecutionContext jobExecutionContext = new GetDefinitionExecutionContext(_jobName);
            var startupStream = new SingleStream<TConfig>(new Tracer(jobExecutionContext, new CurrentExecutionNodeContext(_jobName)), jobExecutionContext, _jobName, PushObservable.Empty<TConfig>());
            _jobDefinition(startupStream);
            return jobExecutionContext.GetDefinitionStructure();
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
        private class GetDefinitionExecutionContext : IExecutionContext
        {
            private List<StreamToNodeLink> _streamToNodeLinks = new List<StreamToNodeLink>();
            private List<string> _nodeNamesToWait = new List<string>();
            public JobDefinitionStructure GetDefinitionStructure()
            {
                return new JobDefinitionStructure(_streamToNodeLinks, _nodeNamesToWait, this.JobName);
            }
            public GetDefinitionExecutionContext(string jobName)
            {
                this.JobName = jobName;
            }
            public void AddStreamToNodeLink(StreamToNodeLink link) => _streamToNodeLinks.Add(link);
            public Guid ExecutionId => throw new NotImplementedException();
            public WaitHandle StartSynchronizer => throw new NotImplementedException();
            public string JobName { get; }
            public bool IsTracingContext => false;
            public void AddDisposable(IDisposable disposable) => throw new NotImplementedException();
            public void AddToWaitForCompletion<T>(string sourceNodeName, IPushObservable<T> stream) => _nodeNamesToWait.Add(sourceNodeName);
            public IPushObservable<TraceEvent> StopProcessEvents => PushObservable.Empty<TraceEvent>();
            public Task GetCompletionTask() => throw new NotImplementedException();
            public void Trace(TraceEvent traceEvent) => throw new NotImplementedException();
        }
        private class JobExecutionContext : IExecutionContext
        {
            public TraceEvent EndOfProcessTraceEvent { get; private set; } = null;
            private readonly IPushSubject<TraceEvent> _traceSubject;
            private List<StreamToNodeLink> _streamToNodeLinks = new List<StreamToNodeLink>();
            private List<string> _nodeNamesToWait = new List<string>();
            public JobDefinitionStructure GetDefinitionStructure()
            {
                return new JobDefinitionStructure(_streamToNodeLinks, _nodeNamesToWait, this.JobName);
            }
            private readonly List<Task> _tasksToWait = new List<Task>();
            private readonly CollectionDisposableManager _disposables = new CollectionDisposableManager();
            public IPushObservable<TraceEvent> StopProcessEvents { get; }
            public JobExecutionContext(string jobName, Guid executionId, IPushSubject<TraceEvent> traceSubject, Func<IPushObservable<TraceEvent>, IPushObservable<TraceEvent>> stopEventFilter)
            {
                this.ExecutionId = executionId;
                this.JobName = jobName;
                this._traceSubject = traceSubject;
                this.StopProcessEvents = stopEventFilter(traceSubject);
                stopEventFilter(traceSubject).Do(traceEvent => this.EndOfProcessTraceEvent = traceEvent);
            }
            public Guid ExecutionId { get; }
            public string JobName { get; }
            public bool IsTracingContext => false;
            public void Trace(TraceEvent traceEvent) => _traceSubject.PushValue(traceEvent);
            public void AddToWaitForCompletion<T>(string sourceNodeName, IPushObservable<T> stream)
            {
                _nodeNamesToWait.Add(sourceNodeName);
                _tasksToWait.Add(stream.ToTaskAsync());
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
            public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
            public void AddStreamToNodeLink(StreamToNodeLink link) => _streamToNodeLinks.Add(link);
        }
        private class TraceExecutionContext : IExecutionContext
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
            public IPushObservable<TraceEvent> StopProcessEvents => PushObservable.Empty<TraceEvent>();
            public void Trace(TraceEvent traveEvent) { }
            public void AddToWaitForCompletion<T>(string sourceNodeName, IPushObservable<T> stream) => _tasksToWait.Add(stream.ToTaskAsync());
            public Task GetCompletionTask() => Task.WhenAll(_tasksToWait.ToArray()).ContinueWith(_ => _disposables.Dispose());
            public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
            public void AddStreamToNodeLink(StreamToNodeLink link) { }
        }
    }
}
