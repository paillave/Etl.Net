using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core
{
    public class StreamToNodeLink(string sourceNodeName, string inputName, string targetNodeName)
    {
        public string InputName { get; set; } = inputName;
        public string TargetNodeName { get; } = targetNodeName;
        public string SourceNodeName { get; } = sourceNodeName;
    }
}
