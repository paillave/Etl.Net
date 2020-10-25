using System.Collections.Generic;
using Paillave.Etl.Core;

namespace Paillave.Etl
{
    public class NodeTraces
    {
        public string NodeName { get; set; }
        public List<TraceEvent> TraceEvents { get; set; }
        public int ActualCount { get; set; }
    }
}