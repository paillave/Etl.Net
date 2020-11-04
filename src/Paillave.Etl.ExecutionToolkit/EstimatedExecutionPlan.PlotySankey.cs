using System.IO;
using System.Linq;
using System.Text.Json;

namespace Paillave.Etl.ExecutionToolkit
{
    public static partial class JobDefinitionStructureEx
    {
        public static PlotlySankeyDescription GetEstimatedExecutionPlan(this JobDefinitionStructure jobDefinitionStructure)
        {
            var nameToIdDictionary = jobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.NodeName, Idx }).ToDictionary(i => i.NodeName, i => i.Idx);
            var links = jobDefinitionStructure.StreamToNodeLinks.Select(link => new
            {
                source = nameToIdDictionary[link.SourceNodeName],
                target = nameToIdDictionary[link.TargetNodeName],
                value = 1
            }
                ).ToList();
            return new PlotlySankeyDescription
            {
                NodeColors = jobDefinitionStructure.Nodes.OrderBy(i => nameToIdDictionary[i.NodeName]).Select(i => "blue").ToList(),
                NodeNames = jobDefinitionStructure.Nodes.OrderBy(i => nameToIdDictionary[i.NodeName]).Select(i => i.NodeName).ToList(),
                LinkSources = links.Select(i => i.source).ToList(),
                LinkTargets = links.Select(i => i.target).ToList(),
                LinkValues = links.Select(i => i.value).ToList()
            };
        }
        public static string GetEstimatedExecutionPlanHtml(this JobDefinitionStructure jobDefinitionStructure)
        {
            var stats = jobDefinitionStructure.GetEstimatedExecutionPlan();
            string file;

            var assembly = typeof(JobDefinitionStructureEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.ExecutionToolkit.Resources.EstimatedExecutionPlan.PlotySankey.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<NODE_NAMES>>'", JsonSerializer.Serialize(stats.NodeNames));
            html = html.Replace("'<<NODE_COLORS>>'", JsonSerializer.Serialize(stats.NodeColors));
            html = html.Replace("'<<LINK_SOURCES>>'", JsonSerializer.Serialize(stats.LinkSources));
            html = html.Replace("'<<LINK_TARGETS>>'", JsonSerializer.Serialize(stats.LinkTargets));
            html = html.Replace("'<<LINK_VALUES>>'", JsonSerializer.Serialize(stats.LinkValues));
            return html;
        }
        public static void OpenEstimatedExecutionPlan(this JobDefinitionStructure jobDefinitionStructure)
        {
            Tools.OpenFile(jobDefinitionStructure.GetEstimatedExecutionPlanHtml(), "html");
        }
    }
}
