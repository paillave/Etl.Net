using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public class StreamToNodeLink
    {
        public StreamToNodeLink(string sourceNodeName, string inputName, string targetNodeName)
        {
            this.InputName = inputName;
            this.TargetNodeName = targetNodeName;
            this.SourceNodeName = sourceNodeName;
        }
        public string InputName { get; set; }
        public string TargetNodeName { get; }
        public string SourceNodeName { get; }
    }
}
