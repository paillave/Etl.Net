using System;
using System.Collections.Generic;
using System.Linq;

namespace Paillave.Etl.Core
{
    public class JobDefinitionStructure
    {
        private class NodeDescription(INodeDescription nodeContext) : INodeDescription
        {
            public string NodeName { get; } = nodeContext.NodeName;
            public string TypeName { get; } = nodeContext.TypeName;
            public ProcessImpact PerformanceImpact { get; } = nodeContext.PerformanceImpact;
            public ProcessImpact MemoryFootPrint { get; } = nodeContext.MemoryFootPrint;
            public INodeDescription Parent => null;
        }
        public JobDefinitionStructure(List<StreamToNodeLink> streamToNodeLinks, List<INodeDescription> nodes, string sourceNodeName)
        {
            this.Nodes = nodes.Select(i => (INodeDescription)new NodeDescription(i))/*.Where(i => i.Parent != null)*/.ToList();
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
