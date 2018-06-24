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
        public NotSortedStreamProcessTrace(IEnumerable<string> sourceNodeName, string side, int lineNumber) : base(sourceNodeName, side, TraceLevel.Error, $"The stream is not sorted at line {lineNumber} whereas is should be")
        {
            this.LineNumber = lineNumber;
        }
    }
}
