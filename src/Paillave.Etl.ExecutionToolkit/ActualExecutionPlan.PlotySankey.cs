using Paillave.Etl.Core;

using System.IO;
using System.Linq;
using System.Text.Json;

namespace Paillave.Etl.ExecutionToolkit;

public static partial class ExecutionStatusEx
{
    private static PlotlySankeyDescription GetActualExecutionPlan(this ExecutionStatus executionStatus)
    {
        var nameToIdDictionary = executionStatus.JobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.NodeName, Idx }).ToDictionary(i => i.NodeName, i => i.Idx);
        var links = executionStatus.JobDefinitionStructure.StreamToNodeLinks.GroupJoin(
                executionStatus.StreamStatisticCounters,
                i => i.SourceNodeName,
                i => i.SourceNodeName,
                (link, stat) => new
                {
                    source = nameToIdDictionary[link.SourceNodeName],
                    target = nameToIdDictionary[link.TargetNodeName],
                    value = stat.DefaultIfEmpty(new StreamStatisticCounter { Counter = 0 }).Sum(i => i.Counter)
                }
            ).ToList();
        return new PlotlySankeyDescription
        {
            NodeColors = executionStatus.JobDefinitionStructure.Nodes.OrderBy(i => nameToIdDictionary[i.NodeName]).Select(i =>
            {
                if (executionStatus.Failed && executionStatus.ErrorTraceEvent != null && executionStatus.ErrorTraceEvent.NodeName == i.NodeName) return "red";
                return "blue";
            }).ToList(),
            NodeNames = executionStatus.JobDefinitionStructure.Nodes.OrderBy(i => nameToIdDictionary[i.NodeName]).Select(i => i.NodeName).ToList(),
            LinkSources = links.Select(i => i.source).ToList(),
            LinkTargets = links.Select(i => i.target).ToList(),
            LinkValues = links.Select(i => i.value).ToList()
        };
    }
    public static string GetActualExecutionPlanHtml(this ExecutionStatus executionStatus)
    {
        var stats = executionStatus.GetActualExecutionPlan();
        string file;

        var assembly = typeof(ExecutionStatusEx).Assembly;

        using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.ExecutionToolkit.Resources.ActualExecutionPlan.PlotySankey.html"))
        using (var reader = new StreamReader(stream))
            file = reader.ReadToEnd();

        string html = file.Replace("'<<NODE_NAMES>>'", JsonSerializer.Serialize(stats.NodeNames));
        html = html.Replace("'<<NODE_COLORS>>'", JsonSerializer.Serialize(stats.NodeColors));
        html = html.Replace("'<<LINK_SOURCES>>'", JsonSerializer.Serialize(stats.LinkSources));
        html = html.Replace("'<<LINK_TARGETS>>'", JsonSerializer.Serialize(stats.LinkTargets));
        html = html.Replace("'<<LINK_VALUES>>'", JsonSerializer.Serialize(stats.LinkValues));
        return html;
    }
    public static void OpenActualExecutionPlan(this ExecutionStatus executionStatus, bool? forceEvenWithNoDebugger = false)
    {
        if (System.Diagnostics.Debugger.IsAttached && !forceEvenWithNoDebugger.Value)
            Tools.OpenFile(executionStatus.GetActualExecutionPlanHtml(), "html");
    }
}
