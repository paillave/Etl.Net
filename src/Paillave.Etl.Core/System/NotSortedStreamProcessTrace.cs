using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class NotSortedStreamProcessTrace : StreamProcessTraceBase
    {
        public int LineNumber { get; private set; }
        public NotSortedStreamProcessTrace(string streamOperatorName, string side, int lineNumber) : base(streamOperatorName, side, TraceLevel.Error, $"The stream is not sorted at line {lineNumber} whereas is should be")
        {
            this.LineNumber = lineNumber;
        }
    }
}
