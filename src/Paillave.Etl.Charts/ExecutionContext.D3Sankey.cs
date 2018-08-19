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
    public static partial class ExecutionContextEx
    {
        public static D3SankeyStatistics GetD3SankeyStatistics(this ExecutionStatus executionStatus)
        {
            var nameToIdDictionary = executionStatus.StreamToNodeLinks.Select(i => i.SourceNodeName).Union(executionStatus.StreamToNodeLinks.Select(i => i.TargetNodeName)).Distinct().Select((name, idx) => new { Name = name, Id = idx }).ToDictionary(i => i.Name, i => i.Id);
            return new D3SankeyStatistics
            {
                links = executionStatus.StreamToNodeLinks.GroupJoin(
                    executionStatus.StreamStatistics,
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
                        value = stat.DefaultIfEmpty(new StreamStatistic { Counter = 0 }).Sum(i => i.Counter)
                    }
                ).ToList(),
                nodes = nameToIdDictionary.Select(i => new D3SankeyStatisticsNode
                {
                    id = i.Value,
                    name = i.Key,
                    color = null
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

            var assembly = typeof(ExecutionContextEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.Charts.d3template.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<SANKEY_STATISTICS>>'", json);
            return html;
        }
        public static void OpenD3SankeyStatistics(this ExecutionStatus executionStatus)
        {
            string tempFilePath=Path.GetTempFileName();
            string htmlTempFilePath = Path.ChangeExtension(tempFilePath, "html");
            File.Move(tempFilePath, htmlTempFilePath);
            File.WriteAllText(htmlTempFilePath, executionStatus.GetHtmlD3SankeyStatistics());
            new Process { StartInfo = new ProcessStartInfo(htmlTempFilePath) { UseShellExecute = true } }.Start();
        }
    }
}
