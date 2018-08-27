using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl
{
    public class JobDefinitionStructure
    {
        public JobDefinitionStructure(List<StreamToNodeLink> streamToNodeLinks, List<string> targetNodeNames, string sourceNodeName)
        {
            this.StreamToNodeLinks = streamToNodeLinks;
            this.Nodes = streamToNodeLinks.Select(i => i.SourceNodeName).Union(streamToNodeLinks.Select(i => i.TargetNodeName)).Distinct().Select(i => new NodeDescription(i, targetNodeNames.Contains(i), i == sourceNodeName)).ToList();
        }
        public List<StreamToNodeLink> StreamToNodeLinks { get; }
        public List<NodeDescription> Nodes { get; }
    }
}
