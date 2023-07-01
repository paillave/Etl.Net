using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public class NodeTraces
    {
        public string NodeName { get; set; }
        public List<TraceEvent> TraceEvents { get; set; }
        public int ActualCount { get; set; }
    }
}