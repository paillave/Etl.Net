using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public interface IExecutionContext
    {
        Guid ExecutionId { get; }
        string JobName { get; }
        void Trace(TraceEvent traceEvent);
        IObservable<TraceEvent> TraceEvents { get; }

        void AddObservableToWait<TRow>(IObservable<TRow> observable);
    }
}
