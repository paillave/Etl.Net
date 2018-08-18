using Paillave.Etl.Core;
using System.Collections.Generic;

namespace Paillave.Etl
{
    public class ExecutionStatus
    {
        private List<StreamToNodeLink> _streamToNodeLinks;
        private List<StreamStatistic> _streamStatistics;
        public ExecutionStatus(List<StreamToNodeLink> streamToNodeLinks, List<StreamStatistic> streamStatistics)
        {
            this._streamToNodeLinks = streamToNodeLinks;
            this._streamStatistics = streamStatistics;
        }
    }
}