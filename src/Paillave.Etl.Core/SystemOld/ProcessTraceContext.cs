using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.SystemOld
{
    public class ProcessTraceContext
    {
        public ProcessTraceContext(Guid executionId, ProcessTrace processTrace)
        {
            this.ExecutionId = executionId;
            this.ProcessTrace = processTrace;
            this.DateTime = DateTime.Now;
        }
        public DateTime DateTime { get; private set; }
        public Guid ExecutionId { get; private set; }
        public ProcessTrace ProcessTrace { get; private set; }
        public override string ToString()
        {
            return $"{this.ExecutionId}-{this.ProcessTrace}";
        }
    }
}
