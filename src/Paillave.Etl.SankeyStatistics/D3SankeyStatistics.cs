using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl
{
    public class D3SankeyStatistics
    {
        public List<D3SankeyStatisticsNode> nodes { get; set; }
        public List<D3SankeyStatisticsLink> links { get; set; }
    }

    public class D3SankeyStatisticsNode
    {
        public int id { get; set; }
        public string name { get; set; }
        public string color { get; set; }
    }

    public class D3SankeyStatisticsLink
    {
        public int source { get; set; }
        public int target { get; set; }
        public int value { get; set; }
    }
}
