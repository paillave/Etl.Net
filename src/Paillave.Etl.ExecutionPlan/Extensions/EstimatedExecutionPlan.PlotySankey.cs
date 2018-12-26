using Newtonsoft.Json;
using Paillave.Etl.ExecutionPlan;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl;

namespace Paillave.Etl.ExecutionPlan.Extensions
{
    public static partial class JobDefinitionStructureEx
    {
        [Obsolete("Use the debugger instead")]
        public static PlotlySankeyDescription GetEstimatedExecutionPlanPlotlySankey(this JobDefinitionStructure jobDefinitionStructure)
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
        //public static async Task<string> GetJsonPlotlySankeyStatisticsAsync<T>(this ExecutionContext<T> executionStatus)
        //{
        //    return JsonConvert.SerializeObject(await executionStatus.GetPlotlySankeyStatisticsAsync());
        //}
        [Obsolete("Use the debugger instead")]
        public static string GetEstimatedExecutionPlanHtmlPlotlySankey(this JobDefinitionStructure jobDefinitionStructure)
        {
            var stats = jobDefinitionStructure.GetEstimatedExecutionPlanPlotlySankey();
            string file;

            var assembly = typeof(JobDefinitionStructureEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.ExecutionPlan.Resources.EstimatedExecutionPlan.PlotySankey.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<NODE_NAMES>>'", JsonConvert.SerializeObject(stats.NodeNames));
            html = html.Replace("'<<NODE_COLORS>>'", JsonConvert.SerializeObject(stats.NodeColors));
            html = html.Replace("'<<LINK_SOURCES>>'", JsonConvert.SerializeObject(stats.LinkSources));
            html = html.Replace("'<<LINK_TARGETS>>'", JsonConvert.SerializeObject(stats.LinkTargets));
            html = html.Replace("'<<LINK_VALUES>>'", JsonConvert.SerializeObject(stats.LinkValues));
            return html;
        }
        [Obsolete("Use the debugger instead")]
        public static void OpenEstimatedExecutionPlanPlotlySankey(this JobDefinitionStructure jobDefinitionStructure)
        {
            Tools.OpenFile(jobDefinitionStructure.GetEstimatedExecutionPlanHtmlPlotlySankey(), "html");
        }
    }
}
