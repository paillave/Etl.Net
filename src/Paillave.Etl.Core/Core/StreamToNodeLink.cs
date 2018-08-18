using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public class StreamToNodeLink
    {
        public StreamToNodeLink(string sourceNodeName, string streamName, string targetNodeName)
        {
            this.StreamName = streamName;
            this.TargetNodeName = targetNodeName;
            this.SourceNodeName = sourceNodeName;
        }
        public string StreamName { get; }
        public string TargetNodeName { get; }
        public string SourceNodeName { get; }
    }
}
