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
        public static D3SankeyDescription GetEstimatedExecutionPlanD3Sankey(this JobDefinitionStructure jobDefinitionStructure)
        {
            var nameToIdDictionary = jobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.Name, Idx }).ToDictionary(i => i.Name, i => i.Idx);
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
                    id = nameToIdDictionary[i.Name],
                    name = i.Name,
                    color = null
                }).ToList()
            };
        }
        public static string GetEstimatedExecutionPlanJsonD3Sankey(this JobDefinitionStructure jobDefinitionStructure)
        {
            return JsonConvert.SerializeObject(jobDefinitionStructure.GetEstimatedExecutionPlanD3Sankey());
        }
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
        public static void OpenEstimatedExecutionPlanD3Sankey(this JobDefinitionStructure jobDefinitionStructure)
        {
            Tools.OpenFile(jobDefinitionStructure.GetEstimatedExecutionPlanHtmlD3Sankey(), "html");
        }
    }
}
