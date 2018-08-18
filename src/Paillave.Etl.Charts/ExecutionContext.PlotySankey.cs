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
        public static async Task<PlotlySankeyStatistics> GetPlotlySankeyStatisticsAsync<T>(this ExecutionContextOld<T> executionContext)
        {
            List<StreamStatistic> streamStatistics = await executionContext.TraceStream.GetStreamStatisticsAsync();
            var streamToNodeLinks = executionContext.StreamToNodeLinks;
            var nameToIdDictionary = streamToNodeLinks.Select(i => i.SourceNodeName).Union(streamToNodeLinks.Select(i => i.TargetNodeName)).Distinct().Select((name, idx) => new { Name = name, Id = idx }).ToDictionary(i => i.Name, i => i.Id);
            var links = streamToNodeLinks.GroupJoin(
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
                    (link, stat) => new
                    {
                        source = nameToIdDictionary[link.SourceNodeName],
                        target = nameToIdDictionary[link.TargetNodeName],
                        value = stat.DefaultIfEmpty(new StreamStatistic { Counter = 0 }).Sum(i => i.Counter)
                    }
                ).ToList();
            return new PlotlySankeyStatistics
            {
                NodeColors = nameToIdDictionary.OrderBy(i => i.Value).Select(i => "blue").ToList(),
                NodeNames = nameToIdDictionary.OrderBy(i => i.Value).Select(i => i.Key).ToList(),
                LinkSources = links.Select(i => i.source).ToList(),
                LinkTargets = links.Select(i => i.target).ToList(),
                LinkValues = links.Select(i => i.value).ToList()
            };
        }
        //public static async Task<string> GetJsonPlotlySankeyStatisticsAsync<T>(this ExecutionContext<T> executionContext)
        //{
        //    return JsonConvert.SerializeObject(await executionContext.GetPlotlySankeyStatisticsAsync());
        //}
        public static async Task<string> GetHtmlPlotlySankeyStatisticsAsync<T>(this ExecutionContextOld<T> executionContext)
        {
            var stats = await executionContext.GetPlotlySankeyStatisticsAsync();
            string file;

            var assembly = typeof(ExecutionContextEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.Charts.plotlytemplate.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<NODE_NAMES>>'", JsonConvert.SerializeObject(stats.NodeNames));
            html = html.Replace("'<<NODE_COLORS>>'", JsonConvert.SerializeObject(stats.NodeColors));
            html = html.Replace("'<<LINK_SOURCES>>'", JsonConvert.SerializeObject(stats.LinkSources));
            html = html.Replace("'<<LINK_TARGETS>>'", JsonConvert.SerializeObject(stats.LinkTargets));
            html = html.Replace("'<<LINK_VALUES>>'", JsonConvert.SerializeObject(stats.LinkValues));
            return html;
        }
    }
}
