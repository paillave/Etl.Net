using System;
using Paillave.Etl.Core;

namespace Paillave.Etl.Core;

public class JobExecutionException(TraceEvent traceEvent) : Exception("Job execution failed", (traceEvent.Content as UnhandledExceptionStreamTraceContent)?.Exception)
{
    public TraceEvent TraceEvent { get; } = traceEvent;
}