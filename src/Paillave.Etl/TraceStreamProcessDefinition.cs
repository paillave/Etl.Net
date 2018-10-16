using System;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl
{
    public class TraceStreamProcessDefinition : ITraceStreamProcessDefinition
    {
        private Action<IStream<TraceEvent>> _defineJob;
        public TraceStreamProcessDefinition(string name, Action<IStream<TraceEvent>> defineJob)
        {
            this.Name = name;
            _defineJob = defineJob;
        }
        public TraceStreamProcessDefinition(Action<IStream<TraceEvent>> defineJob) : this(null, defineJob) { }
        public string Name { get; }
        public void DefineProcess(IStream<TraceEvent> rootStream) => _defineJob(rootStream);
    }
}
