using Newtonsoft.Json;
using Paillave.Etl.ExecutionPlan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Paillave.Etl;
using Paillave.Etl.Core;

namespace Paillave.Etl.ExecutionPlan.Extensions
{
    public static partial class JobDefinitionStructureEx
    {
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static VisNetworkDescription GetEstimatedExecutionPlanVisNetwork(this JobDefinitionStructure jobDefinitionStructure)
        {
            var nameToIdDictionary = jobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.NodeName, Idx }).ToDictionary(i => i.NodeName, i => i.Idx);
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
                        id = nameToIdDictionary[i.NodeName],
                        label = i.NodeName,
                        shape = icon != null ? "icon" : null,
                        icon = icon,
                        color = GetNodeColor(i)
                    };
                }).ToList()
            };
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        private static int GetNodeBorderWidth(INodeContext node)
        {
            // if (node.IsSource) return 8;
            // if (node.IsTarget) return 8;
            return 2;
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        private static VisNetworkStatisticColorNode GetNodeColor(INodeContext node)
        {
            // if (node.IsSource) return new VisNetworkStatisticColorNode { background = "lightgrey", border = "#2B7CE9" };
            // if (node.IsTarget) return new VisNetworkStatisticColorNode { background = "blue", border = "#2B7CE9" };
            return new VisNetworkStatisticColorNode { background = "#D2E5FF", border = "#2B7CE9" };
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        private static VisNetworkStatisticIconNode GetIcon(INodeContext node)
        {
            //if (node.IsSource) return new VisNetworkStatisticIconNode { face = "FontAwesome", size = 50, code = @"\uf2f6" };
            //if (node.IsTarget) return new VisNetworkStatisticIconNode { face = "FontAwesome", size = 50, code = @"\uf2f5" };
            //if (node.IsSource) return new VisNetworkStatisticIconNode { face = "Ionicons", size = 50, code = @"\uf1b1" };
            //if (node.IsTarget) return new VisNetworkStatisticIconNode { face = "Ionicons", size = 50, code = @"\uf1b2" };
            return null;
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static string GetEstimatedExecutionPlanJsonVisNetwork(this JobDefinitionStructure jobDefinitionStructure)
        {
            return JsonConvert.SerializeObject(jobDefinitionStructure.GetEstimatedExecutionPlanVisNetwork()).Replace(@"""\\u", @"""\u");
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static string GetEstimatedExecutionPlanHtmlVisNetwork(this JobDefinitionStructure jobDefinitionStructure)
        {
            var json = jobDefinitionStructure.GetEstimatedExecutionPlanJsonVisNetwork();
            string file;

            var assembly = typeof(ExecutionStatusEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.ExecutionPlan.Resources.EstimatedExecutionPlan.VisNetwork.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<STATISTICS>>'", json);
            return html;
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static void OpenEstimatedExecutionPlanVisNetwork(this JobDefinitionStructure jobDefinitionStructure)
        {
            Tools.OpenFile(jobDefinitionStructure.GetEstimatedExecutionPlanHtmlVisNetwork(), "html");
        }
    }
}
