using Newtonsoft.Json;
using Paillave.Etl.Charts;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl
{
    public static partial class ExecutionContextEx
    {
        public static async Task<VisNetworkStatistics> GetVisNetworkStatisticsAsync<T>(this ExecutionContext<T> executionContext)
        {
            List<StreamStatistic> streamStatistics = await executionContext.TraceStream.GetStreamStatisticsAsync();
            var streamToNodeLinks = executionContext.StreamToNodeLinks;
            var nameToIdDictionary = streamToNodeLinks.Select(i => i.SourceNodeName).Union(streamToNodeLinks.Select(i => i.TargetNodeName)).Distinct().Select((name, idx) => new { Name = name, Id = idx }).ToDictionary(i => i.Name, i => i.Id);
            return new VisNetworkStatistics
            {
                edges = streamToNodeLinks.GroupJoin(
                    streamStatistics,
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
        public static async Task<string> GetJsonVisNetworkStatisticsAsync<T>(this ExecutionContext<T> executionContext)
        {
            return JsonConvert.SerializeObject(await executionContext.GetVisNetworkStatisticsAsync());
        }
        public static async Task<string> GetHtmlVisNetworkStatisticsAsync<T>(this ExecutionContext<T> executionContext)
        {
            var json = await executionContext.GetJsonVisNetworkStatisticsAsync();
            string file;

            var assembly = typeof(ExecutionContextEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.Charts.visnetworktemplate.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<STATISTICS>>'", json);
            return html;
        }
    }
}
