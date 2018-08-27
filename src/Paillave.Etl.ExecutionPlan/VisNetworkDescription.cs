using System.Collections.Generic;

namespace Paillave.Etl
{
    public class VisNetworkDescription
    {
        public List<VisNetworkStatisticEdge> edges { get; set; }
        public List<VisNetworkStatisticNode> nodes { get; set; }
    }

    public class VisNetworkStatisticNode
    {
        public int borderWidth { get; set; }
        public int id { get; set; }
        public int value { get; set; }
        public string label { get; set; }
        public string shape { get; set; }
        public VisNetworkStatisticColorNode color { get; set; }
        public VisNetworkStatisticIconNode icon { get; set; }
    }

    public class VisNetworkStatisticIconNode
    {
        public string face { get; set; }
        public string code { get; set; }
        public int size { get; set; }
        public string color { get; set; }
    }

    public class VisNetworkStatisticEdge
    {
        public int from { get; set; }
        public int to { get; set; }
        public int value { get; set; }
        public string arrows => "to";
        public VisNetworkStatisticColorEdge color { get; set; }
    }

    public class VisNetworkStatisticColorEdge
    {
        public string color { get; set; }
        public bool inherit { get; set; }
    }

    public class VisNetworkStatisticColorNode
    {
        public string border { get; set; }
        public string background { get; set; }
    }
}