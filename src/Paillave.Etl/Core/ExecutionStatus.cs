using System.Collections.Generic;

namespace Paillave.Etl.Core;

public class ExecutionStatus(JobDefinitionStructure jobDefinitionStructure, List<StreamStatisticCounter> streamStatistics, TraceEvent endOfProcessTraceEvent)
{
    public bool Failed => this.ErrorTraceEvent != null;
    public TraceEvent ErrorTraceEvent { get; } = endOfProcessTraceEvent;
    public JobDefinitionStructure JobDefinitionStructure { get; } = jobDefinitionStructure;
    //public List<StreamToNodeLink> StreamToNodeLinks { get; }
    public List<StreamStatisticCounter> StreamStatisticCounters { get; set; } = streamStatistics;
}