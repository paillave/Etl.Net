using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl
{
    internal class CurrentExecutionNodeContext : INodeContext
    {
        public CurrentExecutionNodeContext(string jobName)
        {
            this.NodeName = jobName;
        }
        public string NodeName { get; }
        public string TypeName => "ExecutionContext";
        public bool IsAwaitable => false;
    }
}
