using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public class StreamStatistics
    {
        public List<StreamStatisticCounter> StreamStatisticCounters { get; set; }
        public List<StreamStatisticError> StreamStatisticErrors { get; set; }
    }
}
