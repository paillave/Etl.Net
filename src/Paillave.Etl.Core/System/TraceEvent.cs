using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Humanizer;

namespace Paillave.Etl.Core.System
{
    public class TraceEvent
    {
        public TraceEvent(string jobName, Guid executionId, string nodeTypeName, IEnumerable<string> nodeNamesPath, ITraceContent content)
        {
            this.ExecutionId = executionId;
            this.JobName = jobName;
            this.NodeTypeName = nodeTypeName;
            this.NodeNamesPath = nodeNamesPath;
            this.Content = content;
            this.DateTime = DateTime.Now;
        }
        public Guid ExecutionId { get; }
        public string JobName { get; }
        public DateTime DateTime { get; }

        /// <summary>
        /// If a StreamNode is used by another stream node internaly, log will be aware of it for the trace to be consistent.
        /// The root "user" stream node name is the first item of the enumerable
        /// </summary>
        public IEnumerable<string> NodeNamesPath { get; }
        public string NodeTypeName { get; }

        public ITraceContent Content { get; }
        public override string ToString()
        {
            return $"{this.JobName}/{this.ExecutionId} - [{this.Content.Level}] {NodeTypeName.Dehumanize()}.{string.Join("->", NodeNamesPath.Select(i=>i.Dehumanize()))} : {Content}";
        }
    }
}
