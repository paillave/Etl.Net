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
    //http://resources.jointjs.com/demos/layout
    public static partial class ExecutionStatusEx
    {
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static VisNetworkDescription GetActualExecutionPlan(this ExecutionStatus executionStatus)
        {
            var nameToIdDictionary = executionStatus.JobDefinitionStructure.Nodes.Select((Structure, Idx) => new { Structure.NodeName, Idx }).ToDictionary(i => i.NodeName, i => i.Idx);
            return new VisNetworkDescription
            {
                edges = executionStatus.JobDefinitionStructure.StreamToNodeLinks.GroupJoin(
                    executionStatus.StreamStatisticCounters,
                    i => i.SourceNodeName,
                    i => i.SourceNodeName,
                    (link, stat) => new VisNetworkStatisticEdge
                    {
                        from = nameToIdDictionary[link.SourceNodeName],
                        to = nameToIdDictionary[link.TargetNodeName],
                        value = stat.DefaultIfEmpty(new StreamStatisticCounter { Counter = 0 }).Sum(i => i.Counter),
                        color = new VisNetworkStatisticColorEdge { color = "#ccd5e2", inherit = false }
                    }
                ).ToList(),
                nodes = executionStatus.JobDefinitionStructure.Nodes.GroupJoin(
                    executionStatus.StreamStatisticErrors,
                    i => i.NodeName,
                    i => i.NodeName,
                    (node, errors) =>
                    {
                        var icon = GetNodeIcon(node);
                        bool onError = errors.Any();
                        return new VisNetworkStatisticNode
                        {
                            borderWidth = GetNodeBorderWidth(node, onError),
                            id = nameToIdDictionary[node.NodeName],
                            label = node.NodeName,
                            shape = icon != null ? "icon" : null,
                            icon = icon,
                            color = GetNodeColor(node, onError)
                        };
                    }).ToList()
            };
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        private static int GetNodeBorderWidth(INodeContext node, bool onError)
        {
            if (onError) return 8;
            // if (node.IsSource) return 8;
            // if (node.IsTarget) return 8;
            return 2;
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        private static VisNetworkStatisticColorNode GetNodeColor(INodeContext node, bool onError)
        {
            if (onError) return new VisNetworkStatisticColorNode { background = "salmon", border = "red" };
            // if (node.IsSource) return new VisNetworkStatisticColorNode { background = "lightgrey", border = "#2B7CE9" };
            // if (node.IsTarget) return new VisNetworkStatisticColorNode { background = "blue", border = "#2B7CE9" };
            return new VisNetworkStatisticColorNode { background = "#D2E5FF", border = "#2B7CE9" };
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        private static VisNetworkStatisticIconNode GetNodeIcon(INodeContext node)
        {
            //if (node.IsSource) return new VisNetworkStatisticIconNode { face = "FontAwesome", size = 50, code = @"\uf2f6" };
            //if (node.IsTarget) return new VisNetworkStatisticIconNode { face = "FontAwesome", size = 50, code = @"\uf2f5" };
            //if (node.IsSource) return new VisNetworkStatisticIconNode { face = "Ionicons", size = 50, code = @"\uf1b1" };
            //if (node.IsTarget) return new VisNetworkStatisticIconNode { face = "Ionicons", size = 50, code = @"\uf1b2" };
            return null;
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static string GetActualExecutionPlanJsonVisNetwork(this ExecutionStatus executionStatus)
        {
            return JsonConvert.SerializeObject(executionStatus.GetActualExecutionPlan()).Replace(@"""\\u", @"""\u");
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static string GetActualExecutionPlanHtmlVisNetwork(this ExecutionStatus executionStatus)
        {
            var json = executionStatus.GetActualExecutionPlanJsonVisNetwork();
            string file;

            var assembly = typeof(ExecutionStatusEx).Assembly;

            using (var stream = assembly.GetManifestResourceStream("Paillave.Etl.ExecutionPlan.Resources.ActualExecutionPlan.VisNetwork.html"))
            using (var reader = new StreamReader(stream))
                file = reader.ReadToEnd();

            string html = file.Replace("'<<STATISTICS>>'", json);
            return html;
        }
        [Obsolete("Use the debugger instead (https://github.com/paillave/Etl.Net-Debugger)")]
        public static void OpenActualExecutionPlanVisNetwork(this ExecutionStatus executionStatus)
        {
            Tools.OpenFile(executionStatus.GetActualExecutionPlanHtmlVisNetwork(), "html");
        }
    }
}
