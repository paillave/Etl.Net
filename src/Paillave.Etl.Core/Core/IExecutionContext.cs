using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
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
        WaitHandle StartSynchronizer { get; }
        string JobName { get; }
        void Trace(TraceEvent traceEvent);
        IPushObservable<TraceEvent> TraceEvents { get; }
        void AddToWaitForCompletion<T>(IPushObservable<T> stream);
        void AddDisposable(IDisposable disposable);
        Task GetCompletionTask();
        void AddStreamToNodeLink(StreamToNodeLink link);
        //IPushObservable<TRow> StopIfContextStops<TRow>(IPushObservable<TRow> observable);
    }
}
