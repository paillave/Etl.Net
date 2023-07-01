using System;

namespace Paillave.Etl.Core
{
    public class TraceEvent
    {
        public TraceEvent(string jobName, Guid executionId, string nodeTypeName, string nodeName, ITraceContent content, int sequenceId)
        {
            this.ExecutionId = executionId;
            this.JobName = jobName;
            this.NodeTypeName = nodeTypeName;
            this.NodeName = nodeName;
            this.Content = content;
            this.DateTime = DateTime.Now;
            this.SequenceId = sequenceId;
        }
        public int SequenceId { get; }
        public Guid ExecutionId { get; }
        public string JobName { get; }
        public DateTime DateTime { get; }
        public string NodeName { get; }
        public string NodeTypeName { get; }
        public ITraceContent Content { get; }
        public override string ToString() => $"{this.JobName}/{this.ExecutionId} - [{this.Content.Level}] {NodeName} - {Content}";
    }
}
