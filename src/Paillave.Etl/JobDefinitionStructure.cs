using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl
{
    public class JobDefinitionStructure
    {
        private class NodeDescription : INodeDescription
        {
            public NodeDescription(INodeDescription nodeContext)
            {
                this.NodeName = nodeContext.NodeName;
                this.TypeName = nodeContext.TypeName;
                this.PerformanceImpact = nodeContext.PerformanceImpact;
                this.MemoryFootPrint = nodeContext.MemoryFootPrint;
            }

            public string NodeName { get; }

            public string TypeName { get; }

            public ProcessImpact PerformanceImpact { get; }

            public ProcessImpact MemoryFootPrint { get; }

            public bool IsRootNode => false;
        }
        public JobDefinitionStructure(List<StreamToNodeLink> streamToNodeLinks, List<INodeDescription> nodes, string sourceNodeName)
        {
            this.Nodes = nodes.Select(i => (INodeDescription)new NodeDescription(i)).Where(i => !i.IsRootNode).ToList();
            this.StreamToNodeLinks = streamToNodeLinks
                .Join(this.Nodes, i => i.SourceNodeName, i => i.NodeName, (i, j) => i)
                .Join(this.Nodes, i => i.TargetNodeName, i => i.NodeName, (i, j) => i)
                .ToList();

            // TODO: Redo total PerformanceImpact and MemoryFootPrint computation as it is completely not accurate
            this.PerformanceImpact = this.Nodes.Count == 0 ? ProcessImpact.Light : (ProcessImpact)Math.Round(this.Nodes.Average(i => (decimal)i.PerformanceImpact));
            this.MemoryFootPrint = this.Nodes.Count == 0 ? ProcessImpact.Light : (ProcessImpact)Math.Round(this.Nodes.Average(i => (decimal)i.MemoryFootPrint));
        }
        public List<StreamToNodeLink> StreamToNodeLinks { get; }
        public List<INodeDescription> Nodes { get; }
        public ProcessImpact PerformanceImpact { get; }
        public ProcessImpact MemoryFootPrint { get; }
    }
}
