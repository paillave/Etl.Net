using System;

namespace Paillave.Etl.Core
{
    public class JobExecutionException : Exception
    {
        public TraceEvent TraceEvent { get; }
        public JobExecutionException(TraceEvent traceEvent) : base("Job execution failed")
        {
            this.TraceEvent = traceEvent;
        }
    }
}