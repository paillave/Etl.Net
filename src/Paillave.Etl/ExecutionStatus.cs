using Paillave.Etl.Core;
using System.Collections.Generic;

namespace Paillave.Etl
{
    public class ExecutionStatus
    {
        public bool Failed { get; }
        public List<TraceEvent> ErrorTraceEvents { get; }
        public JobDefinitionStructure JobDefinitionStructure { get; }
        //public List<StreamToNodeLink> StreamToNodeLinks { get; }
        public List<StreamStatisticCounter> StreamStatisticCounters { get; set; }
        public List<StreamStatisticError> StreamStatisticErrors { get; set; }
        public ExecutionStatus(JobDefinitionStructure jobDefinitionStructure, StreamStatistics streamStatistics, List<TraceEvent> errorTraceEvents)
        {
            this.JobDefinitionStructure = jobDefinitionStructure;
            this.StreamStatisticCounters = streamStatistics.StreamStatisticCounters;
            this.StreamStatisticErrors = streamStatistics.StreamStatisticErrors;
            this.ErrorTraceEvents = errorTraceEvents;
            this.Failed = this.ErrorTraceEvents.Count > 0;
        }
    }
}