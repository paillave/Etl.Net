using Newtonsoft.Json;
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
        public static async Task<D3SankeyStatistics> GetD3SankeyStatisticsAsync<T>(this ExecutionContext<T> executionContext)
        {
            List<StreamStatistic> streamStatistics = await executionContext.TraceStream.GetStreamStatisticsAsync();
            var streamToNodeLinks = executionContext.StreamToNodeLinks;
            var nameToIdDictionary = streamToNodeLinks.Select(i => i.SourceNodeName).Union(streamToNodeLinks.Select(i => i.TargetNodeName)).Distinct().Select((name, idx) => new { Name = name, Id = idx }).ToDictionary(i => i.Name, i => i.Id);
            return new D3SankeyStatistics
            {
                links = streamToNodeLinks.GroupJoin(
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
        public static async Task<string> GetJsonD3SankeyStatisticsAsync<T>(this ExecutionContext<T> executionContext)
        {
            return JsonConvert.SerializeObject(await executionContext.GetD3SankeyStatisticsAsync());
        }
        public static async Task<string> GetHtmlD3SankeyStatisticsAsync<T>(this ExecutionContext<T> executionContext)
        {
            var json = await executionContext.GetJsonD3SankeyStatisticsAsync();
            string file;

            var assembly = typeof(ExecutionContextEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.SankeyStatistics.d3template.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<SANKEY_STATISTICS>>'", json);
            return html;
        }
    }
}
