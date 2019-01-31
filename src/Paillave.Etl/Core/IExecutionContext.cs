using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{
    public interface IExecutionContext
    {
        Guid ExecutionId { get; }
        string JobName { get; }
        int NextTraceSequence();
        IPushObservable<TraceEvent> StopProcessEvent { get; }
        void AddNode<T>(INodeContext nodeContext, IPushObservable<T> observable, IPushObservable<TraceEvent> traceObservable);
        void AddDisposable(IDisposable disposable);
        Task GetCompletionTask();
        void AddStreamToNodeLink(StreamToNodeLink link);
        bool IsTracingContext { get; }
        void InvokeInDedicatedThread(object threadOwner, Action action);
    }
}
