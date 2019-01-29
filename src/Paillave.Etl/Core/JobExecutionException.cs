using System;
using Paillave.Etl.Core.TraceContents;

namespace Paillave.Etl.Core
{
    public class JobExecutionException : Exception
    {
        public TraceEvent TraceEvent { get; }
        public JobExecutionException(TraceEvent traceEvent) : base("Job execution failed", (traceEvent.Content as UnhandledExceptionStreamTraceContent)?.Exception)
        {
            this.TraceEvent = traceEvent;
        }
    }
}