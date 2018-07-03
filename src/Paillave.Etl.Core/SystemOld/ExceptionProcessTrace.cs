using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.SystemOld
{
    public class ExceptionProcessTrace : ProcessTrace
    {
        public Exception Exception { get; }

        public ExceptionProcessTrace(IEnumerable<string> sourceNodeName, Exception exception) : base(sourceNodeName, exception.Message, TraceLevel.Error)
        {
            this.Exception = exception;
        }
    }
}
