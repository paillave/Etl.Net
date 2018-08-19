using Paillave.Etl.Core;
using System.Collections.Generic;

namespace Paillave.Etl
{
    public class ExecutionStatus
    {
        public List<StreamToNodeLink> StreamToNodeLinks { get; }
        public List<StreamStatistic> StreamStatistics { get; }
        public ExecutionStatus(List<StreamToNodeLink> streamToNodeLinks, List<StreamStatistic> streamStatistics)
        {
            this.StreamToNodeLinks = streamToNodeLinks;
            this.StreamStatistics = streamStatistics;
        }
    }
}