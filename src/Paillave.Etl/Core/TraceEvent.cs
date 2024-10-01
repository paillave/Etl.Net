using System;

namespace Paillave.Etl.Core
{
    public class TraceEvent(string jobName, Guid executionId, string nodeTypeName, 
        string nodeName, ITraceContent content, int sequenceId)
    {
        public int SequenceId { get; } = sequenceId;
        public Guid ExecutionId { get; } = executionId;
        public string JobName { get; } = jobName;
        public DateTime DateTime { get; } = DateTime.Now;
        public string NodeName { get; } = nodeName;
        public string NodeTypeName { get; } = nodeTypeName;
        public ITraceContent Content { get; } = content;
        public override string ToString() => $"{this.JobName}/{this.ExecutionId} - [{this.Content.Level}] {NodeName} - {Content}";
    }
}
