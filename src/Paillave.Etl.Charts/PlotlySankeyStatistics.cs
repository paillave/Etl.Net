using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Charts
{
    public class PlotlySankeyStatistics
    {
        public List<string> NodeNames { get; set; }
        public List<string> NodeColors { get; set; }
        public List<int> LinkSources { get; set; }
        public List<int> LinkTargets { get; set; }
        public List<int> LinkValues { get; set; }
    }
}
