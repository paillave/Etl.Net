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
    //https://www.strathweb.com/2018/04/generic-and-dynamically-generated-controllers-in-asp-net-core-mvc/
    public class StreamProcessRunner
    {
        public static StreamProcessRunner<TConfig> Create<TConfig>(Action<ISingleStream<TConfig>> jobDefinition, string jobName = "NoName") => new StreamProcessRunner<TConfig>(jobDefinition, jobName);
        public static Task<ExecutionStatus> CreateAndExecuteAsync<TConfig>(TConfig config, Action<ISingleStream<TConfig>> jobDefinition, Action<IStream<TraceEvent>> traceProcessDefinition = null, string jobName = "NoName") => new StreamProcessRunner<TConfig>(jobDefinition, jobName).ExecuteAsync(config, traceProcessDefinition);
        public static Task<ExecutionStatus> CreateAndExecuteWithNoFaultAsync<TConfig>(TConfig config, Action<ISingleStream<TConfig>> jobDefinition, Action<IStream<TraceEvent>> traceProcessDefinition = null, string jobName = "NoName") => new StreamProcessRunner<TConfig>(jobDefinition, jobName).ExecuteWithNoFaultAsync(config, traceProcessDefinition);
    }

    public class StreamProcessRunner<TConfig> : IStreamProcessRunner
    {
        private Action<ISingleStream<TConfig>> _jobDefinition;
        private INodeContext _rootNode;
        private Func<IPushObservable<TraceEvent>, IPushObservable<TraceEvent>> _defaultStopCondition = traces => traces.Filter(i => i.Content.Level == TraceLevel.Error).First();
        public Func<IPushObservable<TraceEvent>, IPushObservable<TraceEvent>> StopCondition { get; set; }
        public StreamProcessRunner(Action<ISingleStream<TConfig>> jobDefinition, string jobName = "NoName")
        {
            _rootNode = new CurrentExecutionNodeContext(jobName);
            _jobDefinition = jobDefinition ?? (_jobDefinition => { });
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
            var jobPoolDispatcher = new JobPoolDispatcher();
            IExecutionContext traceExecutionContext = new TraceExecutionContext(startSynchronizer, executionId, jobPoolDispatcher);
            var traceStream = new Stream<TraceEvent>(null, traceExecutionContext, null, traceSubject);
            JobExecutionContext jobExecutionContext = new JobExecutionContext(_rootNode.NodeName, executionId, traceSubject, this.StopCondition ?? _defaultStopCondition, jobPoolDispatcher);
            var startupStream = new SingleStream<TConfig>(new TraceMapper(jobExecutionContext, _rootNode), jobExecutionContext, _rootNode.NodeName, startupSubject.First());
            traceProcessDefinition?.Invoke(traceStream);
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
                    jobPoolDispatcher.Dispose();
                    return new ExecutionStatus(jobExecutionContext.GetDefinitionStructure(), jobExecutionStatusTask.Result, jobExecutionContext.EndOfProcessTraceEvent);
                });
            startSynchronizer.Set();
            startupSubject.PushValue(config);
            startupSubject.Complete();
            return task;
        }
        public JobDefinitionStructure GetDefinitionStructure()
        {
            GetDefinitionExecutionContext jobExecutionContext = new GetDefinitionExecutionContext(_rootNode);
            var startupStream = new SingleStream<TConfig>(new TraceMapper(jobExecutionContext, _rootNode), jobExecutionContext, _rootNode.NodeName, PushObservable.Empty<TConfig>());
            _jobDefinition(startupStream);
            return jobExecutionContext.GetDefinitionStructure();
        }
    }
}
