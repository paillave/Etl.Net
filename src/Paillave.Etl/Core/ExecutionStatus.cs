using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public class ExecutionStatus
    {
        public bool Failed => this.ErrorTraceEvent != null;
        public TraceEvent ErrorTraceEvent { get; }
        public JobDefinitionStructure JobDefinitionStructure { get; }
        //public List<StreamToNodeLink> StreamToNodeLinks { get; }
        public List<StreamStatisticCounter> StreamStatisticCounters { get; set; }
        public ExecutionStatus(JobDefinitionStructure jobDefinitionStructure, List<StreamStatisticCounter> streamStatistics, TraceEvent endOfProcessTraceEvent)
        {
            this.JobDefinitionStructure = jobDefinitionStructure;
            this.StreamStatisticCounters = streamStatistics;
            this.ErrorTraceEvent = endOfProcessTraceEvent;
        }
    }
}