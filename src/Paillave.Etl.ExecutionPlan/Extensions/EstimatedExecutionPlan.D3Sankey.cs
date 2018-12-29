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
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static D3SankeyDescription GetEstimatedExecutionPlanD3Sankey(this JobDefinitionStructure jobDefinitionStructure)
        {
            var nameToIdDictionary = jobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.NodeName, Idx }).ToDictionary(i => i.NodeName, i => i.Idx);
            return new D3SankeyDescription
            {
                links = jobDefinitionStructure.StreamToNodeLinks.Select(link => new D3SankeyStatisticsLink
                {
                    source = nameToIdDictionary[link.SourceNodeName],
                    target = nameToIdDictionary[link.TargetNodeName],
                    value = 1
                }
                ).ToList(),
                nodes = jobDefinitionStructure.Nodes.Select(i => new D3SankeyStatisticsNode
                {
                    id = nameToIdDictionary[i.NodeName],
                    name = i.NodeName,
                    color = "blue"
                }).ToList()
            };
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static string GetEstimatedExecutionPlanJsonD3Sankey(this JobDefinitionStructure jobDefinitionStructure)
        {
            return JsonConvert.SerializeObject(jobDefinitionStructure.GetEstimatedExecutionPlanD3Sankey());
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static string GetEstimatedExecutionPlanHtmlD3Sankey(this JobDefinitionStructure jobDefinitionStructure)
        {
            var json = jobDefinitionStructure.GetEstimatedExecutionPlanJsonD3Sankey();
            string file;

            var assembly = typeof(ExecutionStatusEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.ExecutionPlan.Resources.EstimatedExecutionPlan.D3Sankey.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<SANKEY_STATISTICS>>'", json);
            return html;
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static void OpenEstimatedExecutionPlanD3Sankey(this JobDefinitionStructure jobDefinitionStructure)
        {
            Tools.OpenFile(jobDefinitionStructure.GetEstimatedExecutionPlanHtmlD3Sankey(), "html");
        }
    }
}
