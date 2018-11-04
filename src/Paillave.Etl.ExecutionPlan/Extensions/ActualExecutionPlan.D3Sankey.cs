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
    public static partial class ExecutionStatusEx
    {
        public static D3SankeyDescription GetActualExecutionPlanD3Sankey(this ExecutionStatus executionStatus)
        {
            var nameToIdDictionary = executionStatus.JobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.Name, Idx }).ToDictionary(i => i.Name, i => i.Idx);
            return new D3SankeyDescription
            {
                links = executionStatus.JobDefinitionStructure.StreamToNodeLinks.GroupJoin(
                    executionStatus.StreamStatisticCounters,
                    i => i.SourceNodeName,
                    i => i.SourceNodeName,
                    (link, stat) => new D3SankeyStatisticsLink
                    {
                        source = nameToIdDictionary[link.SourceNodeName],
                        target = nameToIdDictionary[link.TargetNodeName],
                        value = stat.DefaultIfEmpty(new StreamStatisticCounter { Counter = 0 }).Sum(i => i.Counter)
                    }
                ).ToList(),
                nodes = executionStatus.JobDefinitionStructure.Nodes.Select(i =>
               {
                   string color = "blue";
                   if (executionStatus.StreamStatisticErrors.Any(e => e.NodeName == i.Name)) color = "red";
                   return new D3SankeyStatisticsNode
                   {
                       id = nameToIdDictionary[i.Name],
                       name = i.Name,
                       color = color
                   };
               }).ToList()
            };
        }
        public static string GetActualExecutionPlanJsonD3Sankey(this ExecutionStatus executionStatus)
        {
            return JsonConvert.SerializeObject(executionStatus.GetActualExecutionPlanD3Sankey());
        }
        public static string GetActualExecutionPlanHtmlD3Sankey(this ExecutionStatus executionStatus)
        {
            var json = executionStatus.GetActualExecutionPlanJsonD3Sankey();
            string file;

            var assembly = typeof(ExecutionStatusEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.ExecutionPlan.Resources.ActualExecutionPlan.D3Sankey.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<SANKEY_STATISTICS>>'", json);
            return html;
        }
        public static void OpenActualExecutionPlanD3Sankey(this ExecutionStatus executionStatus)
        {
            Tools.OpenFile(executionStatus.GetActualExecutionPlanHtmlD3Sankey(), "html");
        }
    }
}
