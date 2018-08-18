using System.Collections.Generic;

namespace Paillave.Etl
{
    public class VisNetworkStatistics
    {
        public List<VisNetworkStatisticEdge> edges { get; set; }
        public List<VisNetworkStatisticNode> nodes { get; set; }
    }

    public class VisNetworkStatisticNode
    {
        public int id { get; set; }
        public int value { get; set; }
        public string label { get; set; }
    }

    public class VisNetworkStatisticEdge
    {
        public int from { get; set; }
        public int to { get; set; }
        public int value { get; set; }
        public string arrows => "to";
    }
}