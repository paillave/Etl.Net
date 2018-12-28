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
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static D3SankeyDescription GetActualExecutionPlanD3Sankey(this ExecutionStatus executionStatus)
        {
            var nameToIdDictionary = executionStatus.JobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.NodeName, Idx }).ToDictionary(i => i.NodeName, i => i.Idx);
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
                   if (executionStatus.StreamStatisticErrors.Any(e => e.NodeName == i.NodeName)) color = "red";
                   return new D3SankeyStatisticsNode
                   {
                       id = nameToIdDictionary[i.NodeName],
                       name = i.NodeName,
                       color = color
                   };
               }).ToList()
            };
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static string GetActualExecutionPlanJsonD3Sankey(this ExecutionStatus executionStatus)
        {
            return JsonConvert.SerializeObject(executionStatus.GetActualExecutionPlanD3Sankey());
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
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
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static void OpenActualExecutionPlanD3Sankey(this ExecutionStatus executionStatus)
        {
            Tools.OpenFile(executionStatus.GetActualExecutionPlanHtmlD3Sankey(), "html");
        }
    }
}
