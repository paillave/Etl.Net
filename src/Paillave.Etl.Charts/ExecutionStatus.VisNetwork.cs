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
    public static partial class ExecutionStatusEx
    {
        public static VisNetworkDescription GetVisNetworkStatistics(this ExecutionStatus executionStatus)
        {
            var nameToIdDictionary = executionStatus.JobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.Name, Idx }).ToDictionary(i => i.Name, i => i.Idx);
            return new VisNetworkDescription
            {
                edges = executionStatus.JobDefinitionStructure.StreamToNodeLinks.GroupJoin(
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
                        value = stat.DefaultIfEmpty(new StreamStatistic { Counter = 0 }).Sum(i => i.Counter),
                        color = new VisNetworkStatisticColorEdge { color = "#ccd5e2", inherit = false }
                    }
                ).ToList(),
                nodes = executionStatus.JobDefinitionStructure.Nodes.Select(i =>
                {
                    var icon = GetNodeIcon(i);
                    return new VisNetworkStatisticNode
                    {
                        borderWidth = GetNodeBorderWidth(i),
                        id = nameToIdDictionary[i.Name],
                        label = i.Name,
                        shape = icon != null ? "icon" : null,
                        icon = icon,
                        color = GetNodeColor(i)
                    };
                }).ToList()
            };
        }
        private static int GetNodeBorderWidth(NodeDescription node)
        {
            if (node.IsSource) return 8;
            if (node.IsTarget) return 8;
            return 2;
        }
        private static VisNetworkStatisticColorNode GetNodeColor(NodeDescription node)
        {
            if (node.IsSource) return new VisNetworkStatisticColorNode { background = "lightgrey", border = "#2B7CE9" };
            if (node.IsTarget) return new VisNetworkStatisticColorNode { background = "blue", border = "#2B7CE9" };
            return new VisNetworkStatisticColorNode { background = "#D2E5FF", border = "#2B7CE9" };
        }
        private static VisNetworkStatisticIconNode GetNodeIcon(NodeDescription node)
        {
            //if (node.IsSource) return new VisNetworkStatisticIconNode { face = "FontAwesome", size = 50, code = @"\uf2f6" };
            //if (node.IsTarget) return new VisNetworkStatisticIconNode { face = "FontAwesome", size = 50, code = @"\uf2f5" };
            //if (node.IsSource) return new VisNetworkStatisticIconNode { face = "Ionicons", size = 50, code = @"\uf1b1" };
            //if (node.IsTarget) return new VisNetworkStatisticIconNode { face = "Ionicons", size = 50, code = @"\uf1b2" };
            return null;
        }
        public static string GetJsonVisNetworkStatistics(this ExecutionStatus executionStatus)
        {
            return JsonConvert.SerializeObject(executionStatus.GetVisNetworkStatistics()).Replace(@"""\\u", @"""\u");
        }
        public static string GetHtmlVisNetworkStatistics(this ExecutionStatus executionStatus)
        {
            var json = executionStatus.GetJsonVisNetworkStatistics();
            string file;

            var assembly = typeof(ExecutionStatusEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.Charts.ExecutionStatus.VisNetwork.html"))
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
