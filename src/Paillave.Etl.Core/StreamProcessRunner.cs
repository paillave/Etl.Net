using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.RxPush.Disposables;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    public class StreamProcessRunner<TJob, TConfig> where TJob : IStreamProcessDefinition<TConfig>, new()
    {
        public Task<ExecutionStatus> ExecuteAsync(TConfig config, IStreamProcessDefinition<TraceEvent> traceStreamProcessDefinition = null)
        {
            EventWaitHandle startSynchronizer = new EventWaitHandle(false, EventResetMode.ManualReset);
            IPushSubject<TraceEvent> traceSubject = new PushSubject<TraceEvent>();
            IPushSubject<TConfig> startupSubject = new PushSubject<TConfig>();
            IExecutionContext traceExecutionContext = new TraceExecutionContext(startSynchronizer);
            var traceStream = new Stream<TraceEvent>(null, traceExecutionContext, null, null, traceSubject);
            TJob jobDefinition = new TJob();
            JobExecutionContext jobExecutionContext = new JobExecutionContext(jobDefinition.Name, startSynchronizer, traceSubject);
            var startupStream = new Stream<TConfig>(new Tracer(jobExecutionContext, new CurrentExecutionNodeContext(jobDefinition.Name)), jobExecutionContext, jobDefinition.Name, "Startup", startupSubject.First());

            traceStreamProcessDefinition?.DefineProcess(traceStream);
            jobDefinition.DefineProcess(startupStream);

            Task<List<StreamStatistic>> streamStatisticsTask = traceStream.GetStreamStatisticsAsync();

            startSynchronizer.Set();
            startupSubject.PushValue(config);
            startupSubject.Complete();

            return Task.WhenAll(
                jobExecutionContext
                    .GetCompletionTask()
                    .ContinueWith(_ => traceSubject.Complete()),
                traceExecutionContext.GetCompletionTask())
                .ContinueWith(t => new ExecutionStatus(jobExecutionContext.StreamToNodeLinks, streamStatisticsTask.Result));
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
        private class JobExecutionContext : IExecutionContext
        {
            private readonly IPushSubject<TraceEvent> _traceSubject;
            public List<StreamToNodeLink> StreamToNodeLinks { get; } = new List<StreamToNodeLink>();
            private readonly List<Task> _tasksToWait = new List<Task>();
            private readonly CollectionDisposableManager _disposables = new CollectionDisposableManager();

            public JobExecutionContext(string jobName, WaitHandle startSynchronizer, IPushSubject<TraceEvent> traceSubject)
            {
                this.ExecutionId = new Guid();
                this.JobName = jobName;
                this._traceSubject = traceSubject;
                this.StartSynchronizer = startSynchronizer;
            }
            public Guid ExecutionId { get; }
            public string JobName { get; }
            public IPushObservable<TraceEvent> TraceEvents => _traceSubject;
            public WaitHandle StartSynchronizer { get; }
            public void Trace(TraceEvent traceEvent) => _traceSubject.PushValue(traceEvent);
            public void AddToWaitForCompletion<T>(IPushObservable<T> stream) => _tasksToWait.Add(stream.ToTaskAsync());
            public Task GetCompletionTask() => Task.WhenAll(_tasksToWait.ToArray()).ContinueWith(_ => _disposables.Dispose());
            public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
            public void AddStreamToNodeLink(StreamToNodeLink link) => StreamToNodeLinks.Add(link);
        }
        private class TraceExecutionContext : IExecutionContext
        {
            private readonly IPushObservable<TraceEvent> _traceSubject;
            private readonly List<Task> _tasksToWait = new List<Task>();
            private readonly CollectionDisposableManager _disposables = new CollectionDisposableManager();
            public TraceExecutionContext(WaitHandle startSynchronizer)
            {
                this.ExecutionId = Guid.NewGuid();
                this.JobName = null;
                this._traceSubject = PushObservable.Empty<TraceEvent>(this.StartSynchronizer);
                this.StartSynchronizer = startSynchronizer;
            }
            public Guid ExecutionId { get; }
            public string JobName { get; }
            public IPushObservable<TraceEvent> TraceEvents => _traceSubject;
            public WaitHandle StartSynchronizer { get; }
            public void Trace(TraceEvent traveEvent) { }
            public void AddToWaitForCompletion<T>(IPushObservable<T> stream) => _tasksToWait.Add(stream.ToTaskAsync());
            public Task GetCompletionTask() => Task.WhenAll(_tasksToWait.ToArray()).ContinueWith(_ => _disposables.Dispose());
            public void AddDisposable(IDisposable disposable) => _disposables.Set(disposable);
            public void AddStreamToNodeLink(StreamToNodeLink link) { }
        }
    }
}
