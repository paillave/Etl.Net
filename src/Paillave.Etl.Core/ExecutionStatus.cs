using Paillave.Etl.Core;
using System.Collections.Generic;

namespace Paillave.Etl
{
    public class ExecutionStatus
    {
        public JobDefinitionStructure JobDefinitionStructure { get; }
        //public List<StreamToNodeLink> StreamToNodeLinks { get; }
        public List<StreamStatistic> StreamStatistics { get; }
        public ExecutionStatus(JobDefinitionStructure jobDefinitionStructure, List<StreamStatistic> streamStatistics)
        {
            this.JobDefinitionStructure = jobDefinitionStructure;
            this.StreamStatistics = streamStatistics;
        }
    }
}