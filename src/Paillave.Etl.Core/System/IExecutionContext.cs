using Paillave.Etl.Core.System.Streams;
using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public interface IExecutionContext
    {
        Guid ExecutionId { get; }
        string JobName { get; }
        void Trace(TraceEvent traceEvent);
        IPushObservable<TraceEvent> TraceEvents { get; }
        void WaitCompletion<T>(IPushObservable<T> stream);
        Task GetCompletionTask();
        //IPushObservable<TRow> StopIfContextStops<TRow>(IPushObservable<TRow> observable);
    }
}
