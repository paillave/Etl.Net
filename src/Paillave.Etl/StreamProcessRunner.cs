using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Extensions;
using Paillave.Etl.Connector;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Disposables;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    //https://www.strathweb.com/2018/04/generic-and-dynamically-generated-controllers-in-asp-net-core-mvc/
    public static class StreamProcessRunner
    {
        public static StreamProcessRunner<TConfig> Create<TConfig>(Action<ISingleStream<TConfig>> jobDefinition, string jobName = "Job") => new StreamProcessRunner<TConfig>(jobDefinition, jobName);
        public static Task<ExecutionStatus> CreateAndExecuteAsync<TConfig>(TConfig config, Action<ISingleStream<TConfig>> jobDefinition, ExecutionOptions<TConfig> options = null, string jobName = "Job") => new StreamProcessRunner<TConfig>(jobDefinition, jobName).ExecuteAsync(config, options);
    }
    public class DebugNodeStreamEventArgs : EventArgs
    {
        public DebugNodeStreamEventArgs(string nodeName, int fromSequenceId, int toSequenceId, int count, bool hasError, IList<object> traceContents)
        {
            this.NodeName = nodeName;
            this.FromSequenceId = fromSequenceId;
            this.ToSequenceId = toSequenceId;
            this.Count = count;
            this.HasError = hasError;
            this.TraceContents = new ReadOnlyCollection<object>(traceContents);
        }
        public string NodeName { get; }
        public int FromSequenceId { get; set; }
        public int ToSequenceId { get; set; }
        public int Count { get; set; }
        public bool HasError { get; set; }
        public ReadOnlyCollection<object> TraceContents { get; }
    }
    public class ExecutionOptions<TConfig>
    {
        public bool UseDetailedTraces { get; set; } = false;
        public Action<IStream<TraceEvent>, ISingleStream<TConfig>> TraceProcessDefinition { get; set; } = null;
        public IDependencyResolver Resolver { get; set; } = null;
        public IDependencyResolver TraceResolver { get; set; } = null;
        public bool NoExceptionOnError { get; set; } = false;
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        public IFileValueConnectors Connectors { get; set; } = new NoFileValueConnectors();
    }
    public class StreamProcessRunner<TConfig>
    {
        public delegate void DebugNodeStreamEventHandler(object sender, DebugNodeStreamEventArgs e);
        public event DebugNodeStreamEventHandler DebugNodeStream;

        protected virtual void OnDebugNodeStream(DebugNodeStreamEventArgs e)
        {
            DebugNodeStreamEventHandler handler = DebugNodeStream;
            handler?.Invoke(this, e);
        }
        private Action<ISingleStream<TConfig>> _jobDefinition;
        // private INodeContext _rootNode;
        // private Func<IPushObservable<TraceEvent>, IPushObservable<TraceEvent>> _defaultStopCondition = traces => traces.Filter(i => i.Content.Level == TraceLevel.Error).First();
        public StreamProcessRunner(Action<ISingleStream<TConfig>> jobDefinition, string jobName = "Job")
        {
            _jobDefinition = jobDefinition ?? (_jobDefinition => { });
            JobName = jobName;
        }
        public int DebugChunkSize { get; set; } = 1000;
        public string JobName { get; }

        private void DebugTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<TConfig> startStream)
            => traceStream.Observable
                .Filter(traceEvent => traceEvent.Content is RowProcessStreamTraceContent)
                .Group(traceEvent => traceEvent.NodeName, traceEventStream =>
                    traceEventStream
                        .Chunk(DebugChunkSize)
                        .Do(chunk => this.OnDebugNodeStream(
                            new DebugNodeStreamEventArgs(
                                chunk.First().NodeName,
                                chunk.First().SequenceId,
                                chunk.Last().SequenceId,
                                chunk.Count(),
                                chunk.Any(i => i.Content.Level == System.Diagnostics.TraceLevel.Error),
                                chunk.Select(i => (i.Content as RowProcessStreamTraceContent)?.Row).ToList()))));
        public Task<ExecutionStatus> ExecuteAsync(TConfig config, ExecutionOptions<TConfig> options = null)
        {
            IFileValueConnectors connectors = options?.Connectors ?? new NoFileValueConnectors();
            var internalCancellationTokenSource = new CancellationTokenSource();
            var internalCancellationToken = internalCancellationTokenSource.Token;
            var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(internalCancellationToken, options?.CancellationToken ?? CancellationToken.None);
            var combinedCancellationToken = combinedCancellationTokenSource.Token;

            Guid executionId = Guid.NewGuid();
            EventWaitHandle startSynchronizer = new EventWaitHandle(false, EventResetMode.ManualReset);
            IPushSubject<TraceEvent> traceSubject = new PushSubject<TraceEvent>(CancellationToken.None);
            IPushSubject<TConfig> startupSubject = new PushSubject<TConfig>(combinedCancellationToken);
            var jobPoolDispatcher = new JobPoolDispatcher();
            IExecutionContext traceExecutionContext = new TraceExecutionContext(startSynchronizer, executionId, jobPoolDispatcher, options?.TraceResolver ?? options?.Resolver ?? new DummyDependencyResolver(), CancellationToken.None, connectors);
            JobExecutionContext jobExecutionContext = new JobExecutionContext(this.JobName, executionId, traceSubject, jobPoolDispatcher, options?.Resolver ?? new DummyDependencyResolver(), internalCancellationTokenSource, connectors, options?.UseDetailedTraces ?? false);
            if (options?.CancellationToken != null)
            {
                options.CancellationToken.Register(() => traceSubject.PushValue(new TraceEventFactory(jobExecutionContext).CreateTraceEvent(new CancellationTraceContent(CancellationCause.CancelledFromOutside), jobExecutionContext.NextTraceSequence())));
            }
            internalCancellationToken.Register(() => traceSubject.PushValue(new TraceEventFactory(jobExecutionContext).CreateTraceEvent(new CancellationTraceContent(CancellationCause.CancelledBecauseOfError), jobExecutionContext.NextTraceSequence())));
            var rootNode = new CurrentExecutionNodeContext(this.JobName, new TraceEventFactory(jobExecutionContext), jobExecutionContext);
            var traceStream = new Stream<TraceEvent>(new NotTraceExecutionNodeContext(traceExecutionContext), traceSubject);
            IPushSubject<TConfig> traceStartupSubject = new PushSubject<TConfig>(CancellationToken.None);
            // IPushSubject<TConfig> traceStartupSubject = new PushSubject<TConfig>(combinedCancellationToken);
            var startupStream = new SingleStream<TConfig>(rootNode, startupSubject.First());
            var traceStartupStream = new SingleStream<TConfig>(new NotTraceExecutionNodeContext(traceExecutionContext), traceStartupSubject.First());
            options?.TraceProcessDefinition?.Invoke(traceStream, traceStartupStream);
#if DEBUG
            // DebugTraceProcess(traceStream, traceStartupStream);
#endif
            _jobDefinition(startupStream);
            Task<List<StreamStatisticCounter>> jobExecutionStatusTask = traceStream.GetStreamStatisticsAsync();
            var task = Task.WhenAll(
                jobExecutionContext
                    .GetCompletionTask()
                    .ContinueWith(_ => traceSubject.Complete()),
                traceExecutionContext
                    .GetCompletionTask())
                .ContinueWith(t =>
                {
                    jobExecutionContext.ReleaseResources();
                    jobExecutionStatusTask.Wait();
                    jobPoolDispatcher.Dispose();
                    if (jobExecutionContext.EndOfProcessTraceEvent != null && !(options?.NoExceptionOnError ?? false))
                        throw new JobExecutionException(jobExecutionContext.EndOfProcessTraceEvent);
                    return new ExecutionStatus(jobExecutionContext.GetDefinitionStructure(), jobExecutionStatusTask.Result, jobExecutionContext.EndOfProcessTraceEvent);
                });
            startSynchronizer.Set();
            startupSubject.PushValue(config);
            traceStartupSubject.PushValue(config);
            startupSubject.Complete();
            traceStartupSubject.Complete();
            return task;
        }
        public JobDefinitionStructure GetDefinitionStructure(IFileValueConnectors connectors = null)
        {
            GetDefinitionExecutionContext jobExecutionContext = new GetDefinitionExecutionContext(this.JobName, connectors ?? new NoFileValueConnectors());
            var rootNode = new CurrentExecutionNodeContext(this.JobName, new TraceEventFactory(jobExecutionContext), jobExecutionContext);
            var startupStream = new SingleStream<TConfig>(rootNode, PushObservable.Empty<TConfig>(CancellationToken.None));
            _jobDefinition(startupStream);
            return jobExecutionContext.GetDefinitionStructure();
        }
    }
}
