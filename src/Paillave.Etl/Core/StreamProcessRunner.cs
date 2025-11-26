using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Core;

//https://www.strathweb.com/2018/04/generic-and-dynamically-generated-controllers-in-asp-net-core-mvc/
public static class StreamProcessRunner
{
    public static StreamProcessRunner<TConfig> Create<TConfig>(Action<ISingleStream<TConfig>> jobDefinition)
        => new(jobDefinition);
    public static Task<ExecutionStatus> CreateAndExecuteAsync<TConfig>(TConfig config, Action<ISingleStream<TConfig>> jobDefinition, ExecutionOptions<TConfig> options = null, CancellationToken cancellationToken = default)
        => new StreamProcessRunner<TConfig>(jobDefinition).ExecuteAsync(config, options, cancellationToken);
}
public class DebugNodeStreamEventArgs(string nodeName, int fromSequenceId, int toSequenceId, int count, bool hasError, IList<object> traceContents) : EventArgs
{
    public string NodeName { get; } = nodeName;
    public int FromSequenceId { get; set; } = fromSequenceId;
    public int ToSequenceId { get; set; } = toSequenceId;
    public int Count { get; set; } = count;
    public bool HasError { get; set; } = hasError;
    public ReadOnlyCollection<object> TraceContents { get; } = new ReadOnlyCollection<object>(traceContents);
}
public class ExecutionOptions<TConfig>
{
    public bool UseDetailedTraces { get; set; } = false;
    public Action<IStream<TraceEvent>, ISingleStream<TConfig>>? TraceProcessDefinition { get; set; } = null;
    public IServiceProvider? Services { get; set; } = null;
    public IServiceProvider? TraceServices { get; set; } = null;
    public bool NoExceptionOnError { get; set; } = true;
}
public delegate void DebugNodeStreamEventHandler(object sender, DebugNodeStreamEventArgs e);
public class StreamProcessRunner<TConfig>(Action<ISingleStream<TConfig>> jobDefinition) : IStreamProcessObserver
{
    public event DebugNodeStreamEventHandler? DebugNodeStream = null;

    protected virtual void OnDebugNodeStream(DebugNodeStreamEventArgs e)
    {
        DebugNodeStreamEventHandler? handler = this.DebugNodeStream;
        handler?.Invoke(this, e);
    }
    private readonly Action<ISingleStream<TConfig>> _jobDefinition = jobDefinition ?? (_jobDefinition => { });
    private class CompositeServiceProvider(params IServiceProvider?[] serviceProviders) : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            foreach (var serviceProvider in serviceProviders.Where(sp => sp != null))
            {
                var service = serviceProvider!.GetService(serviceType);
                if (service != null)
                    return service;
            }
            return null;
        }
    }
    public int DebugChunkSize { get; set; } = 1000;
    private void DebugTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<TConfig> startStream)
        => traceStream.Observable
            .Filter(traceEvent => traceEvent.Content is RowProcessStreamTraceContent)
            .Group(traceEvent => traceEvent.NodeName, (traceEventStream, _) =>
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
    public Task<ExecutionStatus> ExecuteAsync(TConfig config, ExecutionOptions<TConfig> options = null, CancellationToken cancellationToken = default)
    {
        var internalCancellationTokenSource = new CancellationTokenSource();
        var internalCancellationToken = internalCancellationTokenSource.Token;
        var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(internalCancellationToken, cancellationToken);
        var combinedCancellationToken = combinedCancellationTokenSource.Token;

        Guid executionId = Guid.NewGuid();
        EventWaitHandle startSynchronizer = new EventWaitHandle(false, EventResetMode.ManualReset);
        IPushSubject<TraceEvent> traceSubject = new PushSubject<TraceEvent>(CancellationToken.None);
        IPushSubject<TConfig> startupSubject = new PushSubject<TConfig>(combinedCancellationToken);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddMemoryCache();
        var internalServices = serviceCollection.BuildServiceProvider();


        var traceExecutionContext = new TraceExecutionContext(
            startSynchronizer,
            executionId,
            new CompositeServiceProvider(internalServices, options?.TraceServices, options?.Services),
            CancellationToken.None);

        var jobExecutionContext = new JobExecutionContext(
            executionId,
            traceSubject,
            new CompositeServiceProvider(internalServices, options?.Services),
            internalCancellationTokenSource,
            (options?.UseDetailedTraces ?? false) || (this.DebugNodeStream != null && Debugger.IsAttached));

        cancellationToken.Register(() => traceSubject.PushValue(new TraceEvent(
            jobExecutionContext.ExecutionId,
            "Job",
            "Job",
            new CancellationTraceContent(CancellationCause.CancelledFromOutside),
            jobExecutionContext.NextTraceSequence())));

        internalCancellationToken.Register(() => traceSubject.PushValue(new TraceEvent(
            jobExecutionContext.ExecutionId,
            "Job",
            "Job",
            new CancellationTraceContent(CancellationCause.CancelledFromOutside),
            jobExecutionContext.NextTraceSequence())));
        var rootNode = new CurrentExecutionNodeContext("<Process>", jobExecutionContext);
        var traceStream = new Stream<TraceEvent>(new NotTraceExecutionNodeContext(traceExecutionContext), traceSubject);
        IPushSubject<TConfig> traceStartupSubject = new PushSubject<TConfig>(CancellationToken.None);
        var startupStream = new SingleStream<TConfig>(rootNode, startupSubject.First(), false);
        var traceStartupStream = new SingleStream<TConfig>(new NotTraceExecutionNodeContext(traceExecutionContext), traceStartupSubject.First());
        options?.TraceProcessDefinition?.Invoke(traceStream, traceStartupStream);
        if (Debugger.IsAttached)
            DebugTraceProcess(traceStream, traceStartupStream);
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
    public JobDefinitionStructure GetDefinitionStructure()
    {
        var jobExecutionContext = new GetDefinitionExecutionContext();
        var rootNode = new CurrentExecutionNodeContext("<Process>", jobExecutionContext);
        var startupStream = new SingleStream<TConfig>(rootNode, PushObservable.Empty<TConfig>(CancellationToken.None));
        _jobDefinition(startupStream);
        return jobExecutionContext.GetDefinitionStructure();
    }
}
