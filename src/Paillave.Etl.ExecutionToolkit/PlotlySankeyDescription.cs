using System.Collections.Generic;

namespace Paillave.Etl.ExecutionToolkit;

public class PlotlySankeyDescription
{
    public List<string> NodeNames { get; set; }
    public List<string> NodeColors { get; set; }
    public List<int> LinkSources { get; set; }
    public List<int> LinkTargets { get; set; }
    public List<int> LinkValues { get; set; }
}
