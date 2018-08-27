using Newtonsoft.Json;
using Paillave.Etl.ExecutionPlan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Paillave.Etl
{
    public static partial class JobDefinitionStructureEx
    {
        public static VisNetworkDescription GetVisNetworkStucture(this JobDefinitionStructure jobDefinitionStructure)
        {
            var nameToIdDictionary = jobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.Name, Idx }).ToDictionary(i => i.Name, i => i.Idx);
            return new VisNetworkDescription
            {
                edges = jobDefinitionStructure.StreamToNodeLinks.Select(link => new VisNetworkStatisticEdge
                {
                    from = nameToIdDictionary[link.SourceNodeName],
                    to = nameToIdDictionary[link.TargetNodeName],
                    value = 1,
                    color = new VisNetworkStatisticColorEdge { color = "#ccd5e2", inherit = false }
                }
                ).ToList(),
                nodes = jobDefinitionStructure.Nodes.Select(i =>
                {
                    var icon = GetIcon(i);
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
        private static VisNetworkStatisticIconNode GetIcon(NodeDescription node)
        {
            //if (node.IsSource) return new VisNetworkStatisticIconNode { face = "FontAwesome", size = 50, code = @"\uf2f6" };
            //if (node.IsTarget) return new VisNetworkStatisticIconNode { face = "FontAwesome", size = 50, code = @"\uf2f5" };
            //if (node.IsSource) return new VisNetworkStatisticIconNode { face = "Ionicons", size = 50, code = @"\uf1b1" };
            //if (node.IsTarget) return new VisNetworkStatisticIconNode { face = "Ionicons", size = 50, code = @"\uf1b2" };
            return null;
        }
        public static string GetJsonVisNetworkStructure(this JobDefinitionStructure jobDefinitionStructure)
        {
            return JsonConvert.SerializeObject(jobDefinitionStructure.GetVisNetworkStucture()).Replace(@"""\\u", @"""\u");
        }
        public static string GetHtmlVisNetworkStructure(this JobDefinitionStructure jobDefinitionStructure)
        {
            var json = jobDefinitionStructure.GetJsonVisNetworkStructure();
            string file;

            var assembly = typeof(ExecutionStatusEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.Charts.JobDefinitionStructure.VisNetwork.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<STATISTICS>>'", json);
            return html;
        }
        public static void OpenVisNetworkStructure(this JobDefinitionStructure jobDefinitionStructure)
        {
            Tools.OpenFile(jobDefinitionStructure.GetHtmlVisNetworkStructure(), "html");
        }
    }
}
