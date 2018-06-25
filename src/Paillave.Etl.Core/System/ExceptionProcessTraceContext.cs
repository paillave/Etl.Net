using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class ExceptionProcessTraceContext : ProcessTraceContext
    {
        public ExceptionProcessTraceContext(Guid executionId, ExceptionProcessTrace processTrace) : base(executionId, processTrace)
        {
        }
        public new ExceptionProcessTrace ProcessTrace => (ExceptionProcessTrace)base.ProcessTrace;
    }
}
