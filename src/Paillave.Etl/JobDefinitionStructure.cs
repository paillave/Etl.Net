using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl
{
    public class JobDefinitionStructure
    {
        private class NodeContext : INodeContext
        {
            public NodeContext(INodeContext nodeContext)
            {
                this.NodeName = nodeContext.NodeName;
                this.TypeName = nodeContext.TypeName;
            }

            public string NodeName { get; }

            public string TypeName { get; }

        }
        public JobDefinitionStructure(List<StreamToNodeLink> streamToNodeLinks, List<INodeContext> nodes, string sourceNodeName)
        {
            this.StreamToNodeLinks = streamToNodeLinks;
            this.Nodes = nodes.Select(i => (INodeContext)new NodeContext(i)).ToList();
        }
        public List<StreamToNodeLink> StreamToNodeLinks { get; }
        public List<INodeContext> Nodes { get; }
    }
}
