using Newtonsoft.Json;
using Paillave.Etl.Charts;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    public static partial class ExecutionStatusEx
    {
        public static D3SankeyDescription GetD3SankeyStatistics(this ExecutionStatus executionStatus)
        {
            var nameToIdDictionary = executionStatus.JobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.Name, Idx }).ToDictionary(i => i.Name, i => i.Idx);
            return new D3SankeyDescription
            {
                links = executionStatus.JobDefinitionStructure.StreamToNodeLinks.GroupJoin(
                    executionStatus.StreamStatisticCounters,
                    i => new
                    {
                        i.SourceNodeName,
                        i.StreamName
                    },
                    i => new
                    {
                        i.SourceNodeName,
                        i.StreamName
                    },
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
        public static string GetJsonD3SankeyStatistics(this ExecutionStatus executionStatus)
        {
            return JsonConvert.SerializeObject(executionStatus.GetD3SankeyStatistics());
        }
        public static string GetHtmlD3SankeyStatistics(this ExecutionStatus executionStatus)
        {
            var json = executionStatus.GetJsonD3SankeyStatistics();
            string file;

            var assembly = typeof(ExecutionStatusEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.Charts.ExecutionStatus.D3Sankey.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<SANKEY_STATISTICS>>'", json);
            return html;
        }
        public static void OpenD3SankeyStatistics(this ExecutionStatus executionStatus)
        {
            Tools.OpenFile(executionStatus.GetHtmlD3SankeyStatistics(), "html");
        }
    }
}
