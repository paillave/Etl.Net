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
    //http://resources.jointjs.com/demos/layout
    public static partial class ExecutionContextEx
    {
        public static VisNetworkStatistics GetVisNetworkStatistics(this ExecutionStatus executionStatus)
        {
            var nameToIdDictionary = executionStatus.StreamToNodeLinks.Select(i => i.SourceNodeName).Union(executionStatus.StreamToNodeLinks.Select(i => i.TargetNodeName)).Distinct().Select((name, idx) => new { Name = name, Id = idx }).ToDictionary(i => i.Name, i => i.Id);
            return new VisNetworkStatistics
            {
                edges = executionStatus.StreamToNodeLinks.GroupJoin(
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
                    (link, stat) => new VisNetworkStatisticEdge
                    {
                        from = nameToIdDictionary[link.SourceNodeName],
                        to = nameToIdDictionary[link.TargetNodeName],
                        value = stat.DefaultIfEmpty(new StreamStatistic { Counter = 0 }).Sum(i => i.Counter)
                    }
                ).ToList(),
                nodes = nameToIdDictionary.Select(i => new VisNetworkStatisticNode
                {
                    id = i.Value,
                    label = i.Key
                }).ToList()
            };
        }
        public static string GetJsonVisNetworkStatistics(this ExecutionStatus executionStatus)
        {
            return JsonConvert.SerializeObject(executionStatus.GetVisNetworkStatistics());
        }
        public static string GetHtmlVisNetworkStatistics(this ExecutionStatus executionStatus)
        {
            var json = executionStatus.GetJsonVisNetworkStatistics();
            string file;

            var assembly = typeof(ExecutionContextEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.Charts.visnetworktemplate.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<STATISTICS>>'", json);
            return html;
        }
        public static void OpenVisNetworkStatistics(this ExecutionStatus executionStatus)
        {
            Tools.OpenFile(executionStatus.GetHtmlVisNetworkStatistics(), "html");
        }
    }
}
